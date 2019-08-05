using System;
using System.Collections.Generic;
using System.Text;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using System.Security;
using SlovakEidSignTool.LowLevelExtensions;

namespace SlovakEidSignTool
{
    public class CardDeviceController : IDisposable
    {
        private readonly Slot slot;
        private readonly Session loginSession;
        private readonly IPinProvider pinProvider;
        private readonly Pkcs11 pkcs11;

        private bool disposedValue = false;

        public CardDeviceController(string pkcs11Libpath, IPinProvider pinProvider, string zepLabel = "SIG_ZEP")
        {
            if (pkcs11Libpath == null) throw new ArgumentNullException(nameof(pkcs11Libpath));
            if (pinProvider == null) throw new ArgumentNullException(nameof(pinProvider));

            this.pinProvider = pinProvider;
            this.pkcs11 = new Pkcs11(pkcs11Libpath, AppType.SingleThreaded);

            try
            {
                List<Slot> slots = this.pkcs11.GetSlotList(SlotsType.WithTokenPresent);
                this.slot = slots.SingleOrDefault(t => string.IsNullOrEmpty(zepLabel) || string.Equals(t.GetTokenInfo().Label, zepLabel, StringComparison.Ordinal));
                if (this.slot == null)
                {
                    this.pkcs11.Dispose();
                    throw new ArgumentException($"PKCS#11 lib '{pkcs11Libpath}' can not contains slot with label '{zepLabel}'.");
                }

                this.loginSession = this.slot.OpenSession(SessionType.ReadOnly);

                if (!this.SessionIsAuthenticated(this.loginSession))
                {
                    SecureString pin = pinProvider.GetBokPin();
                    try
                    {
                        this.loginSession.Login(CKU.CKU_USER, pin);
                    }
                    finally
                    {
                        pin?.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                this.loginSession?.Dispose();
                this.pkcs11.Dispose();
                throw;
            }
        }

        public IReadOnlyList<X509Certificate2> ListCertificates()
        {
            List<X509Certificate2> certificates = new List<X509Certificate2>();
            using (Session session = this.slot.OpenSession(SessionType.ReadOnly))
            {
                foreach (var (_, _, ckaValue) in this.FindCertificates(session))
                {
                    certificates.Add(new X509Certificate2(ckaValue));
                }
            }

            return certificates;
        }

        public IReadOnlyList<CardSigningCertificate> GetSignedCertificates()
        {
            List<CardSigningCertificate> result = new List<CardSigningCertificate>();

            using (Session session = this.slot.OpenSession(SessionType.ReadOnly))
            {
                foreach (var (ckaId, ckaLabel, ckaValue) in this.FindCertificates(session))
                {
                    X509Certificate2 certificate = new X509Certificate2(ckaValue);
                    if (this.IsCertificateForSigning(certificate))
                    {
                        ObjectHandle privateKeyhandle = this.FindPrivateKey(session, ckaId, ckaLabel);
                        if (privateKeyhandle == null)
                        {
                            continue;
                        }

                        List<ObjectAttribute> privateKeyAttr = session.GetAttributeValue(privateKeyhandle, new List<CKA>() { CKA.CKA_ALWAYS_AUTHENTICATE });
                        bool isAwaisAuth = privateKeyAttr[0].GetValueAsBool();

                        result.Add(new CardSigningCertificate(this.slot, ckaValue, privateKeyhandle, isAwaisAuth ? this.pinProvider : null));
                    }
                }
            }

            return result;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private bool IsCertificateForSigning(X509Certificate2 certificate)
        {
            foreach (X509Extension ext in certificate.Extensions)
            {
                if (ext is X509KeyUsageExtension usage)
                {
                    if (usage.KeyUsages.HasFlag(X509KeyUsageFlags.NonRepudiation))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private ObjectHandle FindPrivateKey(Session session, byte[] ckaId, string ckaLabel)
        {
            List<ObjectAttribute> searchTemplate = new List<ObjectAttribute>()
            {
                new ObjectAttribute(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
                new ObjectAttribute(CKA.CKA_TOKEN, true),
                new ObjectAttribute(CKA.CKA_ID, ckaId),
                new ObjectAttribute(CKA.CKA_LABEL, ckaLabel)
            };

            return session.FindAllObjects(searchTemplate).FirstOrDefault();
        }

        private IEnumerable<(byte[] ckaId, string ckaLabel, byte[] ckaValue)> FindCertificates(Session session)
        {
            List<ObjectAttribute> searchTemplate = new List<ObjectAttribute>()
                {
                    new ObjectAttribute(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE),
                    new ObjectAttribute(CKA.CKA_TOKEN, true),
                    new ObjectAttribute(CKA.CKA_CERTIFICATE_TYPE, CKC.CKC_X_509)
                };

            foreach (ObjectHandle certHandle in session.FindAllObjects(searchTemplate))
            {
                List<ObjectAttribute> objectAttributes = session.GetAttributeValue(certHandle, new List<CKA>() { CKA.CKA_ID, CKA.CKA_LABEL, CKA.CKA_VALUE });

                byte[] ckaId = objectAttributes[0].GetValueAsByteArray();
                string ckaLabel = objectAttributes[1].GetValueAsString();
                byte[] ckaValue = objectAttributes[2].GetValueAsByteArray();

                System.Diagnostics.Trace.WriteLine($"Found certificate with CKA_ID: {this.BinToHex(ckaId)}, Label: {ckaLabel}");

                yield return (ckaId, ckaLabel, ckaValue);
            }
        }

        private bool SessionIsAuthenticated(Session session)
        {
            SessionInfo sessionInfo = session.GetSessionInfo();
            switch (sessionInfo.State)
            {
                case CKS.CKS_RO_PUBLIC_SESSION:
                case CKS.CKS_RW_PUBLIC_SESSION:
                    return false;

                case CKS.CKS_RO_USER_FUNCTIONS:
                case CKS.CKS_RW_USER_FUNCTIONS:
                case CKS.CKS_RW_SO_FUNCTIONS:
                    return true;

                default:
                    throw new NotSupportedException($"Session state {sessionInfo.State} is not supported");
            }
        }

        private string BinToHex(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++)
            {
                sb.AppendFormat("{0:X2}", data[i]);
            }

            return data.ToString();
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.loginSession.Dispose();
                    this.pkcs11.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}

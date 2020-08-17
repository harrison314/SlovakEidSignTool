using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool
{
    public class CardSigningCertificate
    {
        private readonly ISlot slot;
        private readonly IObjectHandle privateKeyHandle;
        private readonly IPinProvider pinProvider;

        public byte[] RawCertificate
        {
            get;
            protected set;
        }

        public CardSigningCertificate(ISlot slot, byte[] ckaValue, IObjectHandle privateKeyHandle, IPinProvider pinProvider)
        {
            this.slot = slot ?? throw new ArgumentNullException(nameof(slot));
            this.RawCertificate = ckaValue ?? throw new ArgumentNullException(nameof(ckaValue));
            this.privateKeyHandle = privateKeyHandle ?? throw new ArgumentNullException(nameof(privateKeyHandle));
            this.pinProvider = pinProvider ?? throw new ArgumentNullException(nameof(pinProvider));
        }

        public byte[] SignSHA256Hash(byte[] hash)
        {
            if (hash == null) throw new ArgumentNullException(nameof(hash));
            if (hash.Length != 32) throw new ArgumentOutOfRangeException($"SHA-256 hash has 32 bits");

            // PKCS 1 Digest info for SHA-256, 2.16.840.1.101.3.4.2.1 is oid for SHA-256
            byte[] pkcs1DigestInfo = this.CreateDigestInfo(hash, 
                PkcsExtensions.HashAlgorithmConvertor.ToOid(System.Security.Cryptography.HashAlgorithmName.SHA256));

            using ISession session = this.slot.OpenSession(SessionType.ReadOnly);
            if (this.pinProvider == null)
            {
                using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);
                return session.Sign(mechanism, this.privateKeyHandle, pkcs1DigestInfo);
            }
            else
            {
                SecureString pin = this.pinProvider.GetKepPin();
                try
                {
                    using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);
                    return session.Sign(mechanism, this.privateKeyHandle, pin, pkcs1DigestInfo);
                }
                finally
                {
                    pin?.Dispose();
                }
            }
        }

        public X509Certificate2 GetCertificate()
        {
            return new X509Certificate2(this.RawCertificate);
        }

        private byte[] CreateDigestInfo(byte[] hash, string hashOid)
        {
            DerObjectIdentifier derObjectIdentifier = new DerObjectIdentifier(hashOid);
            AlgorithmIdentifier algorithmIdentifier = new AlgorithmIdentifier(derObjectIdentifier, null);
            DigestInfo digestInfo = new DigestInfo(algorithmIdentifier, hash);
            return digestInfo.GetDerEncoded();
        }
    }
}

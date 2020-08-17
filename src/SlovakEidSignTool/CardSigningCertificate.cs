using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using SlovakEidSignTool.LowLevelExtensions;
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
        private readonly Slot slot;
        private readonly ObjectHandle privateKeyHandle;
        private readonly IPinProvider pinProvider;

        public byte[] RawCertificate
        {
            get;
            protected set;
        }

        public X509Certificate2 ParsedCertificate
        {
            get => new X509Certificate2(this.RawCertificate);
        }

        public CardSigningCertificate(Slot slot, byte[] ckaValue, ObjectHandle privateKeyHandle, IPinProvider pinProvider)
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
            byte[] pkcs1DigestInfo = this.CreateDigestInfo(hash, "2.16.840.1.101.3.4.2.1");

            using Session session = this.slot.OpenSession(SessionType.ReadOnly);
            if (this.pinProvider == null)
            {
                using Mechanism mechanism = new Mechanism(CKM.CKM_RSA_PKCS);
                return session.Sign(mechanism, this.privateKeyHandle, pkcs1DigestInfo);
            }
            else
            {
                SecureString pin = this.pinProvider.GetZepPin();
                try
                {
                    using Mechanism mechanism = new Mechanism(CKM.CKM_RSA_PKCS);
                    return session.SignWithAuth(mechanism, this.privateKeyHandle, pkcs1DigestInfo, pin);
                }
                finally
                {
                    pin?.Dispose();
                }
            }
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

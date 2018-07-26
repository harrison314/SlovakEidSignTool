using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using SlovakEidSignTool.LowLevelExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool
{
    public class CardSignedCertificate
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

        public CardSignedCertificate(Slot slot, byte[] ckaValue, ObjectHandle privateKeyHandle, IPinProvider pinProvider)
        {
            this.slot = slot ?? throw new ArgumentNullException(nameof(slot));
            this.RawCertificate = ckaValue ?? throw new ArgumentNullException(nameof(ckaValue));
            this.privateKeyHandle = privateKeyHandle ?? throw new ArgumentNullException(nameof(privateKeyHandle));
            this.pinProvider = pinProvider;
        }

        public byte[] SignSHA256Hash(byte[] hash)
        {
            if (hash == null) throw new ArgumentNullException(nameof(hash));
            if (hash.Length != 32) throw new ArgumentOutOfRangeException($"SHA-256 hash has 32 bits");

            // PKCS 1 Digest info for SHA-256
            byte[] pkcs1DigestInfo = new byte[] { 0x30, 0x31, 0x30, 0x0D, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01, 0x05, 0x00, 0x04, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Array.Copy(hash, 0, pkcs1DigestInfo, pkcs1DigestInfo.Length - hash.Length, hash.Length);

            using (Session session = this.slot.OpenSession(SessionType.ReadOnly))
            {
                if (this.pinProvider == null)
                {
                    using (Mechanism mechanism = new Mechanism(CKM.CKM_RSA_PKCS))
                    {
                        return session.Sign(mechanism, this.privateKeyHandle, pkcs1DigestInfo);
                    }
                }
                else
                {
                    byte[] pin = this.pinProvider.GetZepPin();

                    using (Mechanism mechanism = new Mechanism(CKM.CKM_RSA_PKCS))
                    {
                        return session.SignWithAuth(mechanism, this.privateKeyHandle, pkcs1DigestInfo, pin);
                    }
                }
            }
        }
    }
}

using iText.Signatures;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool.Pdf
{
    public class Pkcs11ExternalSignature : IExternalSignature
    {
        private readonly CardSigningCertificate cardSignedCertificate;

        public Pkcs11ExternalSignature(CardSigningCertificate cardSignedCertificate)
        {
            this.cardSignedCertificate = cardSignedCertificate ?? throw new ArgumentNullException(nameof(cardSignedCertificate));
        }

        public string GetEncryptionAlgorithm()
        {
            return "RSA";
        }

        public string GetHashAlgorithm()
        {
            return "SHA-256";
        }

        public byte[] Sign(byte[] message)
        {
            byte[] hash = null;
            using (SHA256 hasher = SHA256.Create())
            {
                hash = hasher.ComputeHash(message);
            }

            return this.cardSignedCertificate.SignSHA256Hash(hash);
        }
    }
}

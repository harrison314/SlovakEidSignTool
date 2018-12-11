using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool.Cades
{
    public class CadesExternalSignature : ICadesExternalSignature
    {
        private readonly CardSigningCertificate cardSigningCertificate;

        public RawSignatureType SignatureType
        {
            get => RawSignatureType.Pkcs115;
        }

        public CadesExternalSignature(CardSigningCertificate cardSigningCertificate)
        {
            this.cardSigningCertificate = cardSigningCertificate ?? throw new ArgumentNullException(nameof(cardSigningCertificate));
        }

        public byte[] GetCertificate()
        {
            return this.cardSigningCertificate.RawCertificate;
        }

        public byte[] SignSha256(byte[] signedAttributesDigest)
        {
            if (signedAttributesDigest == null) throw new ArgumentNullException(nameof(signedAttributesDigest));

            return this.cardSigningCertificate.SignSHA256Hash(signedAttributesDigest);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool.Cades
{
    public interface ICadesExternalSignature
    {
        RawSignatureType SignatureType
        {
            get;
        }

        byte[] SignSha256(byte[] signedAttributesDigest);

        byte[] GetCertificate();
    }
}

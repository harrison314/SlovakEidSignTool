using iText.Kernel.Pdf;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool.Pdf
{
    public static class PdfSignerHelper
    {
        public static void Sign(IExternalSignature externalSignature, X509Certificate2 rawCertificate, string sourcePdfPath, string destinationPdfPath)
        {
            if (externalSignature == null) throw new ArgumentNullException(nameof(externalSignature));
            if (rawCertificate == null) throw new ArgumentNullException(nameof(rawCertificate));
            if (sourcePdfPath == null) throw new ArgumentNullException(nameof(sourcePdfPath));
            if (destinationPdfPath == null) throw new ArgumentNullException(nameof(destinationPdfPath));

            using (PdfReader reader = new PdfReader(sourcePdfPath))
            {
                Org.BouncyCastle.X509.X509Certificate bCert = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(rawCertificate);
                Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] { bCert };

                using (FileStream stream = new FileStream(destinationPdfPath, FileMode.OpenOrCreate))
                {
                    PdfSigner signer = new PdfSigner(reader, stream, false);
                    signer.SignDetached(externalSignature, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);
                }
            }
        }

    }
}



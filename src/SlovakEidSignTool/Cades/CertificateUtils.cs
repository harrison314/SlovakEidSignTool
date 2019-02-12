using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool.Cades
{
    internal static class CertificateUtils
    {
        public static IEnumerable<X509Certificate2> BuldChain(X509Certificate2 signedCertificate)
        {
            using (X509Chain chain = new X509Chain())
            {
                if (!chain.Build(signedCertificate))
                {
                    //throw new InvalidOperationException($"Certificate with {signedCertificate.Thumbprint} thumbprint is not valid.");
                }

                foreach (X509ChainElement cert in chain.ChainElements)
                {
                    yield return cert.Certificate;
                }
            }
        }

        public static IEnumerable<byte[]> BuldChain(byte[] signedCertificate)
        {
            if (signedCertificate == null) throw new ArgumentNullException(nameof(signedCertificate));

            return BuldChain(new X509Certificate2(signedCertificate))
                .Select(t => t.RawData);
        }
    }
}

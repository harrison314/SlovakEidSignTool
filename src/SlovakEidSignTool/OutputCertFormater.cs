using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool
{
    public static class OutputCertFormater
    {
        public static string Format(X509Certificate2 certificate, OutputCertFormat format)
        {
            StringBuilder certBuilder = new StringBuilder();
            switch (format)
            {
                case OutputCertFormat.Description:
                    certBuilder.AppendFormat("Thumbprint: {0}", certificate.Thumbprint);
                    certBuilder.AppendLine();
                    certBuilder.AppendFormat("Subject: {0}", certificate.Subject);
                    certBuilder.AppendLine();
                    certBuilder.AppendFormat("Issuer: {0}", certificate.Issuer);
                    certBuilder.AppendLine();
                    break;

                case OutputCertFormat.Pem:

                    certBuilder.AppendLine("-----BEGIN CERTIFICATE-----");
                    certBuilder.AppendLine(Convert.ToBase64String(certificate.RawData, Base64FormattingOptions.InsertLineBreaks));
                    certBuilder.AppendLine("-----END CERTIFICATE-----");

                    break;

                default:
                    throw new InvalidProgramException($"Enum value {format} is not supported.");
            }

            return certBuilder.ToString();
        }
    }
}

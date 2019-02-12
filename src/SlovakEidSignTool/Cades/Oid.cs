using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool.Cades
{
    internal static class Oid
    {
        /// <summary>
        /// PKCS#9 contentType attribute
        /// </summary>
        public static string PKCS9AtContentType = "1.2.840.113549.1.9.3";

        /// <summary>
        /// PKCS#7 data content type
        /// </summary>
        public static string PKCS7IdData = "1.2.840.113549.1.7.1";

        /// <summary>
        /// PKCS#9 messageDigest attribute
        /// </summary>
        public static string PKCS9AtMessageDigest = "1.2.840.113549.1.9.4";

        /// <summary>
        /// PKCS#9 signingTime attribute
        /// </summary>
        public static string PKCS9AtSigningTime = "1.2.840.113549.1.9.5";

        /// <summary>
        /// PKCS#1 RSAES-PKCS-v1_5 signature scheme
        /// </summary>
        public static string PKCS1RsaEncryption = "1.2.840.113549.1.1.1";

        /// <summary>
        /// The SHA256 hash algorithm
        /// </summary>
        public static string SHA256 = "2.16.840.1.101.3.4.2.1";

        /// <summary>
        /// PKCS#7 signed-data content type
        /// </summary>
        public static string PKCS7IdSignedData = "1.2.840.113549.1.7.2";

        /// <summary>
        /// The id-aa-signingCertificateV2. See IETF RFC 5126.
        /// </summary>
        public static string SigningCertificateV2 = "1.2.840.113549.1.9.16.2.47";
    }
}

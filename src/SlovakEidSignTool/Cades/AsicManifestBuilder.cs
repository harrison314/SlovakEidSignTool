using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SlovakEidSignTool.Cades
{
    internal class AsicManifestBuilder
    {
        private readonly XDocument document;
        private readonly XNamespace asic = "http://uri.etsi.org/02918/v1.2.1#";
        private readonly XNamespace dsig = "http://www.w3.org/2000/09/xmldsig#";

        public AsicManifestBuilder()
        {
            this.document = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));
            this.document.Add(new XElement(this.asic + "ASiCManifest",
                new XAttribute(XNamespace.Xmlns + "asic", this.asic.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "dsig", this.dsig.NamespaceName)
                ));
        }

        public void AddP7Signature(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            this.document.Root.Add(new XElement(
                this.asic + "SigReference",
                new XAttribute("URI", path),
                new XAttribute("MimeType", "application/x-pkcs7-signature")
                ));
        }

        public void AddFile(string fileName, string mimeType, Stream content)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (mimeType == null) throw new ArgumentNullException(nameof(mimeType));
            if (content == null) throw new ArgumentNullException(nameof(content));

            string hash = this.GetSha256Encoded(content);

            XElement dataObjectRef = new XElement(
                this.asic + "DataObjectReference",
                new XAttribute("URI", fileName),
                new XAttribute("MimeType", mimeType)
                );

            dataObjectRef.Add(new XElement(
                this.dsig + "DigestMethod",
                new XAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256")
                ));

            dataObjectRef.Add(new XElement(
                this.dsig + "DigestValue",
                hash
                ));

            this.document.Root.Add(dataObjectRef);
        }

        private string GetSha256Encoded(Stream content)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(content);
                return Convert.ToBase64String(hash);
            }
        }

        public byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (TextWriter textWriter = new StreamWriter(ms, Encoding.UTF8, 2048, true))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter))
                    {
                        this.document.Save(xmlWriter);
                        xmlWriter.Flush();
                        textWriter.Flush();
                    }
                }

                return ms.ToArray();
            }
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(this.ToByteArray());
        }
    }
}

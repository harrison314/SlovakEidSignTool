using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool.Cades
{
    public abstract class CadesSigner
    {
        protected readonly List<(FileInfo, string)> inputFiles;

        public CadesSigner()
        {
            this.inputFiles = new List<(FileInfo, string)>();
        }

        public void AddFile(FileInfo fileInfo, string mimeType)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (mimeType == null)
            {
                throw new ArgumentNullException(nameof(mimeType));
            }

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("File not exits.", fileInfo.FullName);
            }

            this.inputFiles.Add((fileInfo, mimeType));
        }

        public abstract void CreateContainer(ICadesExternalSignature externalSigner, string ouputFilePath);

        protected void AddFileToArchive(ZipArchive archive, string name, string content)
        {
            ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(name);
            byte[] contentData = Encoding.UTF8.GetBytes(content);

            using (Stream stream = zipArchiveEntry.Open())
            {
                stream.Write(contentData, 0, contentData.Length);
            }
        }

        protected void AddFileToArchive(ZipArchive archive, string name, byte[] content)
        {
            ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(name);

            using (Stream stream = zipArchiveEntry.Open())
            {
                stream.Write(content, 0, content.Length);
            }
        }
        protected void AddFileToArchive(ZipArchive archive, string name, FileInfo contentFile)
        {
            ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(name);

            using (Stream fileStream = contentFile.OpenRead())
            {
                using (Stream stream = zipArchiveEntry.Open())
                {
                    fileStream.CopyTo(stream);
                }
            }
        }

        protected IEnumerable<X509Certificate> BuildCertificatePath(X509Certificate signingCertificate)
        {
            //TODO: refactor

            Org.BouncyCastle.X509.X509CertificateParser x509CertificateParser = new X509CertificateParser();

            return CertificateUtils.BuldChain(signingCertificate.GetEncoded())
                 .Select(t => x509CertificateParser.ReadCertificate(t));
        }
    }
}

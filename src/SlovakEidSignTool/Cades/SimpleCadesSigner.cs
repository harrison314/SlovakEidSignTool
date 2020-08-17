using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.X509;

namespace SlovakEidSignTool.Cades
{
    public class SimpleCadesSigner : CadesSigner
    {
        private const string SignaturePath = "META-INF/signatureSkEid.p7s";
        private const string ManifestPath = "META-INF/ASiCManifestSkEid.xml";
        private const string ContainerMimeType = "application/vnd.etsi.asic-e+zip";
        private const string ContainerMimeTypePath = "mimetype";

        public SimpleCadesSigner()
            : base()
        {

        }

        public override void CreateContainer(ICadesExternalSignature externalSigner, string ouputFilePath)
        {
            if (externalSigner == null)
            {
                throw new ArgumentNullException(nameof(externalSigner));
            }

            if (ouputFilePath == null)
            {
                throw new ArgumentNullException(nameof(ouputFilePath));
            }

            AsicManifestBuilder asicManifestBuilder = new AsicManifestBuilder();
            asicManifestBuilder.AddP7Signature(SignaturePath);

            foreach ((FileInfo file, string mimeType) in this.inputFiles)
            {
                using Stream contentStream = file.OpenRead();
                asicManifestBuilder.AddFile(file.Name, mimeType, contentStream);
            }

            byte[] manifestData = asicManifestBuilder.ToByteArray();

            Pkcs7DetachedSignatureGenerator p7Generator = new Pkcs7DetachedSignatureGenerator(externalSigner);

            X509CertificateParser x509CertificateParser = new X509CertificateParser();
            X509Certificate signingCertificate = x509CertificateParser.ReadCertificate(externalSigner.GetCertificate());

            byte[] signature = p7Generator.GenerateP7s(manifestData, signingCertificate, this.BuildCertificatePath(signingCertificate));

            using ZipArchive archive = ZipFile.Open(ouputFilePath, ZipArchiveMode.Create);
            this.AddFileToArchive(archive, ContainerMimeTypePath, ContainerMimeType);
            this.AddFileToArchive(archive, ManifestPath, manifestData);
            this.AddFileToArchive(archive, SignaturePath, signature);

            foreach ((FileInfo file, _) in this.inputFiles)
            {
                this.AddFileToArchive(archive, file.Name, file);
            }
        }
    }
}

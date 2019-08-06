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
    public class ExtendedCadesSigner : CadesSigner
    {
        private const string ContainerMimeTypePath = "mimetype";

        private readonly string originalAsicePath;

        public ExtendedCadesSigner(string originalAsicePath)
            : base()
        {
            this.originalAsicePath = originalAsicePath ?? throw new ArgumentNullException(nameof(SimpleCadesSigner));

            if (!File.Exists(originalAsicePath))
            {
                throw new FileNotFoundException("Asic-e file to extend not found.", originalAsicePath);
            }
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

            File.Copy(this.originalAsicePath, ouputFilePath);
            try
            {
                using (ZipArchive archive = ZipFile.Open(ouputFilePath, ZipArchiveMode.Update))
                {
                    (string signaturePath, string manifestPath) = this.CreateMetadatNames(archive);
                    AsicManifestBuilder asicManifestBuilder = new AsicManifestBuilder();
                    asicManifestBuilder.AddP7Signature(signaturePath);

                    foreach ((FileInfo file, string mimeType) in this.inputFiles)
                    {
                        using (Stream contentStream = file.OpenRead())
                        {
                            asicManifestBuilder.AddFile(file.Name, mimeType, contentStream);
                        }
                    }

                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.FullName.StartsWith("META-INF/", StringComparison.Ordinal) && !string.Equals(entry.FullName, ContainerMimeTypePath, StringComparison.Ordinal))
                        {
                            using (Stream contentStream = entry.Open())
                            {
                                asicManifestBuilder.AddFile(entry.FullName, MimeType.GetMimeTypeFromFileName(Path.GetFileName(entry.FullName)), contentStream);
                            }
                        }
                    }

                    byte[] manifestData = asicManifestBuilder.ToByteArray();
                    X509CertificateParser x509CertificateParser = new X509CertificateParser();
                    X509Certificate signingCertificate = x509CertificateParser.ReadCertificate(externalSigner.GetCertificate());

                    Pkcs7DetachedSignatureGenerator p7Generator = new Pkcs7DetachedSignatureGenerator(externalSigner);
                    byte[] signature = p7Generator.GenerateP7s(manifestData, signingCertificate, this.BuildCertificatePath(signingCertificate));

                    this.AddFileToArchive(archive, manifestPath, manifestData);
                    this.AddFileToArchive(archive, signaturePath, signature);

                    foreach ((FileInfo file, _) in this.inputFiles)
                    {
                        this.AddFileToArchive(archive, file.Name, file);
                    }
                }
            }
            catch
            {
                if (File.Exists(ouputFilePath))
                {
                    File.Delete(ouputFilePath);
                }

                throw;
            }
        }

        private (string signaturePath, string manifestPath) CreateMetadatNames(ZipArchive archive)
        {
            int index = 1;
            string signaturePath, manifestPath;
            while (index < 50000)
            {
                signaturePath = $"META-INF/signatureSkEid{index}.p7s";
                if (archive.GetEntry(signaturePath) != null)
                {
                    continue;
                }

                manifestPath = $"META-INF/ASiCManifestSkEid{index}.xml";
                if (archive.GetEntry(manifestPath) != null)
                {
                    continue;
                }

                return (signaturePath, manifestPath);
            }

            throw new InvalidDataException("Can not create a new signature");
        }
    }
}

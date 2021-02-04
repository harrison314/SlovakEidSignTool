using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Ess;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;

namespace SlovakEidSignTool.Cades
{
    internal class Pkcs7DetachedSignatureGenerator
    {
        private readonly ICadesExternalSignature cadesExternalSignature;

        public Pkcs7DetachedSignatureGenerator(ICadesExternalSignature cadesExternalSignature)
        {
            this.cadesExternalSignature = cadesExternalSignature ?? throw new ArgumentNullException(nameof(cadesExternalSignature));
        }

        public byte[] GenerateP7s(byte[] data, X509Certificate signingCertificate, IEnumerable<X509Certificate> certPath)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (signingCertificate == null)
            {
                throw new ArgumentNullException(nameof(signingCertificate));
            }

            if (certPath == null)
            {
                throw new ArgumentNullException(nameof(certPath));
            }

            byte[] dataHashSha256 = this.ComputeHash(data);
            Asn1EncodableVector signedAttributesVector = this.CreateSignatureAttribute(dataHashSha256, signingCertificate);

            DerSet signedAttributes = new DerSet(signedAttributesVector);
            byte[] signedAttributesDigest = this.ComputeHash(signedAttributes.GetDerEncoded());

            byte[] signature = this.cadesExternalSignature.SignSha256(signedAttributesDigest);
            DerOctetString digestSignature = new DerOctetString(signature);
            AlgorithmIdentifier digestSignatureAlgorithm = this.CreateSignatureInfo();

            SignerInfo signerInfo = new SignerInfo(
                sid: new SignerIdentifier(new IssuerAndSerialNumber(signingCertificate.IssuerDN, signingCertificate.SerialNumber)),
                digAlgorithm: new AlgorithmIdentifier(
                    algorithm: new DerObjectIdentifier(Oid.SHA256),
                    parameters: DerNull.Instance
                ),
                authenticatedAttributes: signedAttributes,
                digEncryptionAlgorithm: digestSignatureAlgorithm,
                encryptedDigest: digestSignature,
                unauthenticatedAttributes: null
            );

            // Construct SignedData.digestAlgorithms
            Asn1EncodableVector digestAlgorithmsVector = new Asn1EncodableVector();
            digestAlgorithmsVector.Add(
                new AlgorithmIdentifier(
                    algorithm: new DerObjectIdentifier(Oid.SHA256),
                    parameters: DerNull.Instance));

            // Construct SignedData.encapContentInfo
            ContentInfo encapContentInfo = new ContentInfo(
                contentType: new DerObjectIdentifier(Oid.PKCS7IdData),
                content: null);

            // Construct SignedData.certificates
            Asn1EncodableVector certificatesVector = new Asn1EncodableVector();
            foreach (X509Certificate cert in certPath)
            {
                certificatesVector.Add(X509CertificateStructure.GetInstance(Asn1Object.FromByteArray(cert.GetEncoded())));
            }

            // Construct SignedData.signerInfos
            Asn1EncodableVector signerInfosVector = new Asn1EncodableVector();
            signerInfosVector.Add(signerInfo.ToAsn1Object());

            // Construct SignedData
            SignedData signedData = new SignedData(
                digestAlgorithms: new DerSet(digestAlgorithmsVector),
                contentInfo: encapContentInfo,
                certificates: new BerSet(certificatesVector),
                crls: null,
                signerInfos: new DerSet(signerInfosVector));

            // Construct top level ContentInfo
            ContentInfo contentInfo = new ContentInfo(
                contentType: new DerObjectIdentifier(Oid.PKCS7IdSignedData),
                content: signedData);

            return contentInfo.GetDerEncoded();
        }

        private Asn1EncodableVector CreateSignatureAttribute(byte[] dataHashSha256, X509Certificate signingCertificate)
        {
            Asn1EncodableVector signedAttributesVector = new Asn1EncodableVector();
            signedAttributesVector.Add(
                new Org.BouncyCastle.Asn1.Cms.Attribute(
                    attrType: new DerObjectIdentifier(Oid.PKCS9AtContentType),
                    attrValues: new DerSet(new DerObjectIdentifier(Oid.PKCS7IdData))));

            // Add PKCS#9 messageDigest signed attribute
            signedAttributesVector.Add(
                new Org.BouncyCastle.Asn1.Cms.Attribute(
                    attrType: new DerObjectIdentifier(Oid.PKCS9AtMessageDigest),
                    attrValues: new DerSet(new DerOctetString(dataHashSha256))));

            // Add PKCS#9 signingTime signed attribute
            signedAttributesVector.Add(
                new Org.BouncyCastle.Asn1.Cms.Attribute(
                    attrType: new DerObjectIdentifier(Oid.PKCS9AtSigningTime),
                    attrValues: new DerSet(new Org.BouncyCastle.Asn1.Cms.Time(new DerUtcTime(DateTime.UtcNow)))));

            // Add SigningCertificateV2
            byte[] certHash = this.ComputeHash(signingCertificate.GetEncoded());
            EssCertIDv2 essCert1 = new EssCertIDv2(new AlgorithmIdentifier(new DerObjectIdentifier(Oid.SHA256)), certHash);
            SigningCertificateV2 scv2 = new SigningCertificateV2(new EssCertIDv2[] { essCert1 });

            signedAttributesVector.Add(
                new Org.BouncyCastle.Asn1.Cms.Attribute(
                    attrType: new DerObjectIdentifier(Oid.SigningCertificateV2),
                    attrValues: new DerSet(scv2)));


            return signedAttributesVector;
        }

        private AlgorithmIdentifier CreateSignatureInfo()
        {
            switch (this.cadesExternalSignature.SignatureType)
            {
                case RawSignatureType.Pkcs115:
                    {
                        AlgorithmIdentifier digestSignatureAlgorithm = new AlgorithmIdentifier(
                        algorithm: new DerObjectIdentifier(Oid.PKCS1RsaEncryption),
                        parameters: DerNull.Instance);

                        return digestSignatureAlgorithm;
                    }

                case RawSignatureType.RsaSaaPss:
                    {
                        // Construct SignerInfo.signatureAlgorithm
                        AlgorithmIdentifier digestSignatureAlgorithmForPss = new AlgorithmIdentifier(
                            algorithm: new DerObjectIdentifier("1.2.840.113549.1.1.10"),
                            parameters: new Org.BouncyCastle.Asn1.Pkcs.RsassaPssParameters(
                                hashAlgorithm: new AlgorithmIdentifier(
                                    algorithm: new DerObjectIdentifier(Oid.SHA256),
                                    parameters: DerNull.Instance
                                ),
                                maskGenAlgorithm: new AlgorithmIdentifier(
                                    algorithm: new DerObjectIdentifier("1.2.840.113549.1.1.8"),
                                    parameters: new AlgorithmIdentifier(
                                        algorithm: new DerObjectIdentifier(Oid.SHA256),
                                        parameters: DerNull.Instance
                                    )
                                ),
                                saltLength: new DerInteger(256 / 8),
                                trailerField: new DerInteger(1)
                            )
                        );

                        return digestSignatureAlgorithmForPss;
                    }

                default:
                    throw new InvalidProgramException($"Enum value {this.cadesExternalSignature.SignatureType} is not supported.");
            }
        }

        private byte[] ComputeHash(byte[] data)
        {
            using System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();
            return sha256.ComputeHash(data);
        }
    }
}

using CommandLine;
using SlovakEidSignTool.Cades;
using SlovakEidSignTool.Pdf;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SlovakEidSignTool
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ListCertOptions, SignPdfOptions, SignCadesOptions, AddSignCadesOptions>(args)
               .MapResult(
                    (ListCertOptions opts) => ListCertificates(opts),
                    (SignPdfOptions opts) => SignPdf(opts),
                    (SignCadesOptions opts) => SignCades(opts),
                    (AddSignCadesOptions opts) => AddSignCades(opts),
                    _ => 1);
        }

        private static int ListCertificates(ListCertOptions opts)
        {
            string eidLib = string.IsNullOrEmpty(opts.LibPath) ? FindEidLibrary() : opts.LibPath;
            Console.WriteLine("Load: {0}", eidLib);
            Console.WriteLine("Certificates:");
            Console.WriteLine();

            using (CardDeviceController cardDeviceController = new CardDeviceController(eidLib, CreatePinprovider(opts.UseAppPinInput), opts.ListEp ? "SIG_EP" : "SIG_ZEP"))
            {
                foreach (X509Certificate2 certificate in cardDeviceController.ListCertificates())
                {
                    Console.WriteLine(OutputCertFormater.Format(certificate, opts.OutputFormat));
                }
            }

            return 0;
        }

        private static int SignPdf(SignPdfOptions opts)
        {
            string eidLib = string.IsNullOrEmpty(opts.LibPath) ? FindEidLibrary() : opts.LibPath;
            Console.WriteLine("Load: {0}", eidLib);
            using (CardDeviceController cardDeviceController = new CardDeviceController(eidLib, CreatePinprovider(opts.UseAppPinInput)))
            {
                CardSigningCertificate signedCertificate = cardDeviceController.GetSignedCertificates().Single();
                Console.WriteLine("Signing certificate with subject: {0}", signedCertificate.ParsedCertificate.Subject);

                Pkcs11ExternalSignature pkcs11ExternalSignature = new Pkcs11ExternalSignature(signedCertificate);

                PdfSignerHelper.Sign(pkcs11ExternalSignature,
                    signedCertificate.ParsedCertificate,
                    opts.SourcePdf,
                    opts.DestinationFile);

                Console.WriteLine("{0} signed and saved to {1}", Path.GetFileName(opts.SourcePdf), opts.DestinationFile);
            }

            return 0;
        }

        private static int SignCades(SignCadesOptions opts)
        {
            string eidLib = string.IsNullOrEmpty(opts.LibPath) ? FindEidLibrary() : opts.LibPath;
            Console.WriteLine("Load: {0}", eidLib);
            using (CardDeviceController cardDeviceController = new CardDeviceController(eidLib, CreatePinprovider(opts.UseAppPinInput)))
            {
                CardSigningCertificate signedCertificate = cardDeviceController.GetSignedCertificates().Single();
                Console.WriteLine("Signing certificate with subject: {0}", signedCertificate.ParsedCertificate.Subject);

                ICadesExternalSignature externalSignature = new CadesExternalSignature(signedCertificate);
                SimpleCadesSigner signer = new SimpleCadesSigner();

                string mimeType = string.IsNullOrEmpty(opts.SourceFileMimeType) ? MimeType.GetMimeTypeFromFileName(Path.GetFileName(opts.SourceFile)) : opts.SourceFileMimeType;
                signer.AddFile(new FileInfo(opts.SourceFile), mimeType);

                signer.CreateContainer(externalSignature, opts.DestinationFile);

                Console.WriteLine("{0} signed and saved to {1}", Path.GetFileName(opts.SourceFile), opts.DestinationFile);
            }

            return 0;
        }

        private static int AddSignCades(AddSignCadesOptions opts)
        {
            string eidLib = string.IsNullOrEmpty(opts.LibPath) ? FindEidLibrary() : opts.LibPath;
            Console.WriteLine("Load: {0}", eidLib);
            using (CardDeviceController cardDeviceController = new CardDeviceController(eidLib, CreatePinprovider(opts.UseAppPinInput)))
            {
                CardSigningCertificate signedCertificate = cardDeviceController.GetSignedCertificates().Single();
                Console.WriteLine("Signing certificate with subject: {0}", signedCertificate.ParsedCertificate.Subject);

                ICadesExternalSignature externalSignature = new CadesExternalSignature(signedCertificate);
                ExtendedCadesSigner signer = new ExtendedCadesSigner(opts.ContainerFile);

                if (!string.IsNullOrEmpty(opts.SourceFile))
                {
                    string mimeType = string.IsNullOrEmpty(opts.SourceFileMimeType) ? MimeType.GetMimeTypeFromFileName(Path.GetFileName(opts.SourceFile)) : opts.SourceFileMimeType;
                    signer.AddFile(new FileInfo(opts.SourceFile), mimeType);
                }

                signer.CreateContainer(externalSignature, opts.DestinationFile);

                Console.WriteLine("Add signature to {0} and saved to {1}", Path.GetFileName(opts.ContainerFile), opts.DestinationFile);
            }

            return 0;
        }

        private static string FindEidLibrary()
        {
            string[] paths = new string[]
            {
                $@"C:\Program Files (x86)\eID klient\pkcs11_{(IntPtr.Size == 4 ? "x86" : "x64")}.dll",
                $@"C:\Program Files\eID klient\pkcs11_{(IntPtr.Size == 4 ? "x86" : "x64")}.dll",

                $@"C:/Program Files/EAC MW klient/pkcs11_{(IntPtr.Size == 4 ? "x86" : "x64")}.dll",
                $@"C:/Program Files (x86)/EAC MW klient/pkcs11_{(IntPtr.Size == 4 ? "x86" : "x64")}.dll",

                $@"/usr/lib/eidklient/libpkcs11_sig_{(IntPtr.Size == 4 ? "x86" : "x64")}.so"
                // /Applications/eIDklient.app/Contents/Pkcs11/libPkcs11.dylib
            };

            foreach (string potentialPath in paths)
            {
                try
                {
                    if (Path.IsPathFullyQualified(potentialPath) && File.Exists(potentialPath))
                    {
                        return potentialPath;
                    }
                }
                catch (Exception ex)
                {
                    // Ignore exception
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
            }

            throw new IOException("Not found PKCS#11 library.");
        }

        private static IPinProvider CreatePinprovider(bool useAppPinInput)
        {
            if (useAppPinInput)
            {
                return new ConsolePinProvider();
            }
            else
            {
                return new EidPinProvider();
            }
        }
    }
}

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
                    (ListCertOptions opts) => HandleSpecificErrors(opts, ListCertificates),
                    (SignPdfOptions opts) => HandleSpecificErrors(opts, SignPdf),
                    (SignCadesOptions opts) => HandleSpecificErrors(opts, SignCades),
                    (AddSignCadesOptions opts) => HandleSpecificErrors(opts, AddSignCades),
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
            using CardDeviceController cardDeviceController = new CardDeviceController(eidLib, CreatePinprovider(opts.UseAppPinInput));
            CardSigningCertificate signedCertificate = cardDeviceController.GetSignedCertificates().Single();
            using X509Certificate2 certificate = signedCertificate.GetCertificate();
            Console.WriteLine("Signing certificate with subject: {0}", certificate.Subject);

            Pkcs11ExternalSignature pkcs11ExternalSignature = new Pkcs11ExternalSignature(signedCertificate);

            PdfSignerHelper.Sign(pkcs11ExternalSignature,
                certificate,
                opts.SourcePdf,
                opts.DestinationFile);

            Console.WriteLine("{0} signed and saved to {1}", Path.GetFileName(opts.SourcePdf), opts.DestinationFile);

            return 0;
        }

        private static int SignCades(SignCadesOptions opts)
        {
            string eidLib = string.IsNullOrEmpty(opts.LibPath) ? FindEidLibrary() : opts.LibPath;
            Console.WriteLine("Load: {0}", eidLib);
            using CardDeviceController cardDeviceController = new CardDeviceController(eidLib, CreatePinprovider(opts.UseAppPinInput));
            CardSigningCertificate signedCertificate = cardDeviceController.GetSignedCertificates().Single();
            using X509Certificate2 certificate = signedCertificate.GetCertificate();
            Console.WriteLine("Signing certificate with subject: {0}", certificate.Subject);

            ICadesExternalSignature externalSignature = new CadesExternalSignature(signedCertificate);
            SimpleCadesSigner signer = new SimpleCadesSigner();

            string mimeType = string.IsNullOrEmpty(opts.SourceFileMimeType) ? MimeType.GetMimeTypeFromFileName(Path.GetFileName(opts.SourceFile)) : opts.SourceFileMimeType;
            signer.AddFile(new FileInfo(opts.SourceFile), mimeType);

            signer.CreateContainer(externalSignature, opts.DestinationFile);

            Console.WriteLine("{0} signed and saved to {1}", Path.GetFileName(opts.SourceFile), opts.DestinationFile);

            return 0;
        }

        private static int AddSignCades(AddSignCadesOptions opts)
        {
            string eidLib = string.IsNullOrEmpty(opts.LibPath) ? FindEidLibrary() : opts.LibPath;
            Console.WriteLine("Load: {0}", eidLib);
            using CardDeviceController cardDeviceController = new CardDeviceController(eidLib, CreatePinprovider(opts.UseAppPinInput));
            CardSigningCertificate signedCertificate = cardDeviceController.GetSignedCertificates().Single();
            using X509Certificate2 certificate = signedCertificate.GetCertificate();
            Console.WriteLine("Signing certificate with subject: {0}", certificate.Subject);

            ICadesExternalSignature externalSignature = new CadesExternalSignature(signedCertificate);
            ExtendedCadesSigner signer = new ExtendedCadesSigner(opts.ContainerFile);

            if (!string.IsNullOrEmpty(opts.SourceFile))
            {
                string mimeType = string.IsNullOrEmpty(opts.SourceFileMimeType) ? MimeType.GetMimeTypeFromFileName(Path.GetFileName(opts.SourceFile)) : opts.SourceFileMimeType;
                signer.AddFile(new FileInfo(opts.SourceFile), mimeType);
            }

            signer.CreateContainer(externalSignature, opts.DestinationFile);

            Console.WriteLine("Add signature to {0} and saved to {1}", Path.GetFileName(opts.ContainerFile), opts.DestinationFile);

            return 0;
        }

        private static string FindEidLibrary()
        {
            string[] paths = new string[]
            {
                $@"C:\Program Files (x86)\eID klient\pkcs11_{(IntPtr.Size == 4 ? "x86" : "x64")}.dll",
                $@"C:\Program Files\eID klient\pkcs11_{(IntPtr.Size == 4 ? "x86" : "x64")}.dll",

                $@"C:\Program Files (x86)\eID_klient\pkcs11_{(IntPtr.Size == 4 ? "x86" : "x64")}.dll",
                $@"C:\Program Files\eID_klient\pkcs11_{(IntPtr.Size == 4 ? "x86" : "x64")}.dll",

                $@"C:/Program Files/EAC MW klient/pkcs11_{(IntPtr.Size == 4 ? "x86" : "x64")}.dll",
                $@"C:/Program Files (x86)/EAC MW klient/pkcs11_{(IntPtr.Size == 4 ? "x86" : "x64")}.dll",

                $@"/usr/lib/eidklient/libpkcs11_sig_{(IntPtr.Size == 4 ? "x86" : "x64")}.so",

                "/Applications/eIDklient.app/Contents/Pkcs11/libPkcs11.dylib",
                "/Applications/Aplikacia_pre_eID.app/Contents/Pkcs11/libPkcs11.dylib"
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

        private static int HandleSpecificErrors<T>(T options, Func<T, int> action)
        {
            try
            {
                return action(options);
            }
            catch (Net.Pkcs11Interop.Common.Pkcs11Exception ex) when (ex.RV == Net.Pkcs11Interop.Common.CKR.CKR_FUNCTION_CANCELED)
            {
                Console.WriteLine("Operation canseled by user.");
                return 1;
            }
            catch (Net.Pkcs11Interop.Common.Pkcs11Exception ex) when (ex.RV == Net.Pkcs11Interop.Common.CKR.CKR_PIN_INVALID)
            {
                Console.WriteLine("PIN has invalid.");
                return 1;
            }
            catch (Net.Pkcs11Interop.Common.Pkcs11Exception ex) when (ex.RV == Net.Pkcs11Interop.Common.CKR.CKR_PIN_INCORRECT)
            {
                Console.WriteLine("PIN has incorrect.");
                return 1;
            }
        }
    }
}

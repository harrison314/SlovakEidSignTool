using CommandLine;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SlovakEidSignTool
{
    public class Program
    {
        public static int Main(string[] args)
        {

            return Parser.Default.ParseArguments<ListCertOptions, SignPdfOptions>(args)
               .MapResult(
                    (ListCertOptions opts) => ListCertificates(opts),
                    (SignPdfOptions opts) => SignPdf(opts),
                    _ => 1);
        }

        private static int ListCertificates(ListCertOptions opts)
        {
            string eidLib = string.IsNullOrEmpty(opts.LibPath) ? FindEidLibrary() : opts.LibPath;
            Console.WriteLine("Load: {0}", eidLib);
            Console.WriteLine("Certificates:");
            Console.WriteLine();

            using (CardDeviceController cardDeviceController = new CardDeviceController(eidLib, new ConsolePinprovider()))
            {
                foreach (X509Certificate2 certificate in cardDeviceController.ListCertificates())
                {
                    Console.WriteLine("Thumbprint: {0}", certificate.Thumbprint);
                    Console.WriteLine("Subject: {0}", certificate.Subject);
                    Console.WriteLine("Issuer: {0}", certificate.Issuer);
                    Console.WriteLine();
                }
            }

            return 0;
        }

        private static int SignPdf(SignPdfOptions opts)
        {
            string eidLib = string.IsNullOrEmpty(opts.LibPath) ? FindEidLibrary() : opts.LibPath;
            Console.WriteLine("Load: {0}", eidLib);
            using (CardDeviceController cardDeviceController = new CardDeviceController(eidLib, new ConsolePinprovider()))
            {
                CardSignedCertificate signedCertificate = cardDeviceController.GetSignedCertificates().Single();
                Console.WriteLine("Sign certificate with subject: {0}", signedCertificate.ParsedCertificate.Subject);

                Pkcs11ExternalSignature pkcs11ExternalSignature = new Pkcs11ExternalSignature(signedCertificate);

                PdfSignerHelper.Sign(pkcs11ExternalSignature,
                    signedCertificate.ParsedCertificate,
                    opts.SourcePdf,
                    opts.DestinationPdf);

                Console.WriteLine("{0} signed and saved to {1}", System.IO.Path.GetFileName(opts.SourcePdf), opts.DestinationPdf);
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
                catch(Exception ex)
                {
                    // Ignore exception
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
            }

            throw new Exception("Not found PKCS#11 library.");
        }
    }
}

using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace SlovakEidSignTool
{
    [Verb("signPDF", HelpText = "Naive sign PDF using eID.")]
    public class SignPdfOptions
    {
        [Option('l', "pkcs11Lib", Default = null, HelpText = "Full path to PKCS#11 library.")]
        public string LibPath
        {
            get;
            set;
        }

        [Option('p', "useAppPinInput", Default = null, HelpText = "Use this application to set PINs.")]
        public bool UseAppPinInput
        {
            get;
            set;
        }

        [Value(0, MetaName = "sourcePDF", HelpText = "Source PDF file path.")]
        public string SourcePdf
        {
            get;
            set;
        }

        [Value(1, MetaName = "destinationPDF", HelpText = "Destination PDF file path for save signed PDF.")]
        public string DestinationPdf
        {
            get;
            set;
        }

        public SignPdfOptions()
        {

        }

        [Usage(ApplicationAlias = "dotnet SlovakEidSignTool.dll")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Sign PDF", new SignPdfOptions() { SourcePdf = "helloWorld.pdf", DestinationPdf = "helloWorld_signed.pdf", UseAppPinInput = false });
                yield return new Example("Sign PDF with application PIN typing", new SignPdfOptions() { SourcePdf = "helloWorld.pdf", DestinationPdf = "helloWorld_signed.pdf", UseAppPinInput = true });
                yield return new Example("Sign PDF with specific PKCS11 lib", new SignPdfOptions() { SourcePdf = "helloWorld.pdf", DestinationPdf = "helloWorld_signed.pdf", LibPath = "/opt/SkEid/bin/pkcs11_x64.so", UseAppPinInput = false });
            }
        }
    }
}

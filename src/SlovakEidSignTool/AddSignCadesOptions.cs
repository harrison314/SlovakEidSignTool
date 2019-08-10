using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace SlovakEidSignTool
{
    [Verb("addSignCades", HelpText = "Add CAdES-BASELINE-B signature to ASIC-E container using eID.")]
    public class AddSignCadesOptions
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

        [Option('m', "mimeType", HelpText = "Source file mime-type. Eg. text/plain, image/png,...", Required = false, Default = null)]
        public string SourceFileMimeType
        {
            get;
            set;
        }

        [Value(0, MetaName = "asiceFile", HelpText = "ASIC-E container (*.asice) to add sign.")]
        public string ContainerFile
        {
            get;
            set;
        }

        [Value(1, MetaName = "sourceFile", HelpText = "Source file path.", Required = false)]
        public string SourceFile
        {
            get;
            set;
        }

        [Option('o', "destinationFile", HelpText = "Destination file path for save signed .asice file.", Required = true)]
        public string DestinationFile
        {
            get;
            set;
        }

        public AddSignCadesOptions()
        {

        }

        [Usage(ApplicationAlias = "dotnet SlovakEidSignTool.dll")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Sign text document", new AddSignCadesOptions() { ContainerFile = "signedDocument.asice", SourceFile = "hello.pdf", DestinationFile = "hello.asice", LibPath = null, UseAppPinInput = false });
                yield return new Example("Sign text document with mime-type", new AddSignCadesOptions() { ContainerFile = "signedDocument.asice", SourceFile = "hello.pdf", SourceFileMimeType = "plain/text", DestinationFile = "hello.asice", LibPath = null, UseAppPinInput = false });
                yield return new Example("Sign PNG image with application PIN typing", new AddSignCadesOptions() { ContainerFile = "signedDocument.asice", SourceFile = "hello.png", SourceFileMimeType = "image/png", DestinationFile = "hello.asice", UseAppPinInput = true });
                yield return new Example("Sign PDF with specific PKCS11 lib", new AddSignCadesOptions() { ContainerFile = "signedDocument.asice", SourceFile = "document.pdf", SourceFileMimeType = "application/pdf", DestinationFile = "document.asice", LibPath = "/opt/SkEid/bin/pkcs11_x64.so", UseAppPinInput = false });
            }
        }
    }
}

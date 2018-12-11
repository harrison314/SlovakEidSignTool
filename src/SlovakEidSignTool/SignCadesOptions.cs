using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool
{
    [Verb("signCades", HelpText = "Sign file using eID to CAdES ASIC-E container.")]
    public class SignCadesOptions
    {
        [Option('l', "pkcs11Lib", Default = null, HelpText = "Full path to PKCS#11 library.")]
        public string LibPath
        {
            get;
            set;
        }

        [Option('e', "useEidClient", Default = null, HelpText = "Use eID client to set PINs.")]
        public bool UseEidClientPin
        {
            get;
            set;
        }

        [Value(0, MetaName = "sourceFile", HelpText = "Source file path.")]
        public string SourceFile
        {
            get;
            set;
        }

        [Value(1, MetaName = "mimeType", HelpText = "Source file mime-type.")]
        public string SourceFileMimeType
        {
            get;
            set;
        }

        [Value(2, MetaName = "destinationFile", HelpText = "Destination file path for save signed asice file.")]
        public string DestinationFile
        {
            get;
            set;
        }

        public SignCadesOptions()
        {

        }

        [Usage(ApplicationAlias = "dotnet SlovakEidSignTool.dll")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Sign text document", new SignCadesOptions() { SourceFile = "hello.pdf", SourceFileMimeType = "plain/text", DestinationFile = "hello.asice", LibPath = null, UseEidClientPin = false });
                yield return new Example("Sign PNG image with eID client for PIN typing", new SignCadesOptions() { SourceFile = "hello.png", SourceFileMimeType = "image/png", DestinationFile = "hello.asice", UseEidClientPin = true });
                yield return new Example("Sign PDF with specific PKCS11 lib", new SignCadesOptions() { SourceFile = "document.pdf", SourceFileMimeType = "application/pdf", DestinationFile = "document.asice", LibPath = "/opt/SkEid/bin/pkcs11_x64.so", UseEidClientPin = false });
            }
        }
    }
}

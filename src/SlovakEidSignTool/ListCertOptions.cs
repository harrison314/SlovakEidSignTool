using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace SlovakEidSignTool
{
    [Verb("list", HelpText = "List certificates in eID.")]
    public class ListCertOptions
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

        [Option('f', "outputFormat", Default = OutputCertFormat.Description, HelpText = "Output certificate format. Allowed: Description, Pem.")]
        public OutputCertFormat OutputFormat
        {
            get;
            set;
        }

        [Option('a', "listEp", Default = false, HelpText = "List ZIP_EP certificates.")]
        public bool ListEp
        {
            get;
            set;
        }

        public ListCertOptions()
        {

        }

        [Usage(ApplicationAlias = "dotnet SlovakEidSignTool.dll")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("List certificates", new ListCertOptions());
                yield return new Example("List certificates with specific PKCS11 lib", new ListCertOptions() { LibPath = "/opt/SkEid/bin/pkcs11_x64.so" });
            }
        }
    }
}

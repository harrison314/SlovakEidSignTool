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

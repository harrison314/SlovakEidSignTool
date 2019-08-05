using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool
{
    public class EidPinProvider : IPinProvider
    {
        public EidPinProvider()
        {

        }

        public SecureString GetBokPin()
        {
            Console.WriteLine("Type BOK using eID client");
            return null;
        }

        public SecureString GetZepPin()
        {
            Console.WriteLine("Type KEP PIN using eID client");
            return null;
        }
    }
}

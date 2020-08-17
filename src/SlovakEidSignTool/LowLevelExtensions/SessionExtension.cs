using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using PkcsExtensions;

namespace SlovakEidSignTool.LowLevelExtensions
{
    public static class SessionExtension
    {
        public static void Login(this ISession session, CKU userType, SecureString securePin)
        {
            if (securePin == null)
            {
                session.Login(userType, pin: null as byte[]);
            }
            else
            {
                SecureStringHelper.ExecuteWithSecureString(securePin, Encoding.UTF8, pin => session.Login(userType, pin));
            }
        }

        public static byte[] Sign(this ISession session, IMechanism mechanism, IObjectHandle objectHandle, SecureString securePin, byte[] data)
        {
            if (mechanism == null) throw new ArgumentNullException(nameof(mechanism));
            if (objectHandle == null) throw new ArgumentNullException(nameof(objectHandle));
            if (data == null) throw new ArgumentNullException(nameof(data));


            return SecureStringHelper.ExecuteWithSecureString<byte[]>(securePin, Encoding.UTF8, pin => session.Sign(mechanism, objectHandle, pin, data));
        }
    }
}

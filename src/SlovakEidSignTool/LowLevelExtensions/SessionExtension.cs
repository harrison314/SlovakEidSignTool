using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

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
                SecurityUtils.ExecuteUsingSecureUtf8String(securePin, pin => session.Login(userType, pin));
            }
        }

        public static byte[] Sign(this ISession session, IMechanism mechanism, IObjectHandle objectHandle, byte[] data, SecureString securePin)
        {
            if (mechanism == null) throw new ArgumentNullException(nameof(mechanism));
            if (objectHandle == null) throw new ArgumentNullException(nameof(objectHandle));
            if (data == null) throw new ArgumentNullException(nameof(data));

            byte[] signature = null;
            SecurityUtils.ExecuteUsingSecureUtf8String(securePin, 
                pin => signature = session.Sign(mechanism, objectHandle, pin, data));

            return signature;

        }
    }
}

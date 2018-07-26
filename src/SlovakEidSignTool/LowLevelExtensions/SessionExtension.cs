using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace SlovakEidSignTool.LowLevelExtensions
{
    public static class SessionExtension
    {
        public static byte[] SignWithAuth(this Session session, Mechanism mechanism, ObjectHandle objectHandle, byte[] data, byte[] pin)
        {
            if (mechanism == null) throw new ArgumentNullException(nameof(mechanism));
            if (objectHandle == null) throw new ArgumentNullException(nameof(objectHandle));
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (Platform.UnmanagedLongSize == 4)
            {
                if (Platform.StructPackingSize == 0)
                {
                    //81
                    Net.Pkcs11Interop.HighLevelAPI81.Mechanism mechanism81 = (Net.Pkcs11Interop.HighLevelAPI81.Mechanism)typeof(Mechanism)
                        .GetField("_mechanism81", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(mechanism);
                    Net.Pkcs11Interop.HighLevelAPI81.ObjectHandle objecthandle81 = (Net.Pkcs11Interop.HighLevelAPI81.ObjectHandle) typeof(ObjectHandle)
                        .GetField("_objectHandle81", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(objectHandle);
                   return session.HLA81Session.SignWithPin(mechanism81, objecthandle81, data, pin);

                }
                else
                {
                    //41
                    Net.Pkcs11Interop.HighLevelAPI41.Mechanism mechanism41 = (Net.Pkcs11Interop.HighLevelAPI41.Mechanism)typeof(Mechanism)
                        .GetField("_mechanism41", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(mechanism);
                    Net.Pkcs11Interop.HighLevelAPI41.ObjectHandle objecthandle41 = (Net.Pkcs11Interop.HighLevelAPI41.ObjectHandle)typeof(ObjectHandle)
                        .GetField("_objectHandle41", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(objectHandle);
                    return session.HLA41Session.SignWithPin(mechanism41, objecthandle41, data, pin);
                }
            }
            else
            {
                if (Platform.StructPackingSize == 0)
                {
                    //80
                    Net.Pkcs11Interop.HighLevelAPI80.Mechanism mechanism80 = (Net.Pkcs11Interop.HighLevelAPI80.Mechanism)typeof(Mechanism)
                        .GetField("_mechanism80", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(mechanism);
                    Net.Pkcs11Interop.HighLevelAPI80.ObjectHandle objecthandle80 = (Net.Pkcs11Interop.HighLevelAPI80.ObjectHandle)typeof(ObjectHandle)
                        .GetField("_objectHandle80", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(objectHandle);
                    return session.HLA80Session.SignWithPin(mechanism80, objecthandle80, data, pin);
                }
                else
                {
                    //81
                    Net.Pkcs11Interop.HighLevelAPI81.Mechanism mechanism81 = (Net.Pkcs11Interop.HighLevelAPI81.Mechanism)typeof(Mechanism)
                        .GetField("_mechanism81", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(mechanism);
                    Net.Pkcs11Interop.HighLevelAPI81.ObjectHandle objecthandle81 = (Net.Pkcs11Interop.HighLevelAPI81.ObjectHandle)typeof(ObjectHandle)
                        .GetField("_objectHandle81", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(objectHandle);
                    return session.HLA81Session.SignWithPin(mechanism81, objecthandle81, data, pin);
                }
            }
        }
    }
}

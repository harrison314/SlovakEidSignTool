using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI81;
using Net.Pkcs11Interop.LowLevelAPI81;

namespace SlovakEidSignTool.LowLevelExtensions
{
    internal static class Session81Extension
    {
        public static byte[] SignWithPin(this Session session, Mechanism mechanism, ObjectHandle keyHandle, byte[] data, byte[] pin)
        {
            CK_MECHANISM ckMechanism = (CK_MECHANISM)typeof(Mechanism).GetField("_ckMechanism", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(mechanism);
            Net.Pkcs11Interop.LowLevelAPI81.Pkcs11 _p11 = session.LowLevelPkcs11;
            CKR rv = _p11.C_SignInit(session.SessionId, ref ckMechanism, keyHandle.ObjectId);
            if (rv != CKR.CKR_OK)
            {
                throw new Pkcs11Exception("C_SignInit", rv);
            }

            rv = _p11.C_Login(session.SessionId, CKU.CKU_CONTEXT_SPECIFIC, pin, Convert.ToUInt64(pin.Length));
            if (rv != CKR.CKR_OK)
            {
                throw new Pkcs11Exception("C_Login", rv);
            }

            ulong signatureLen = 0;
            rv = _p11.C_Sign(session.SessionId, data, Convert.ToUInt32(data.Length), null, ref signatureLen);
            if (rv != CKR.CKR_OK)
            {
                throw new Pkcs11Exception("C_Sign", rv);
            }

            byte[] signature = new byte[signatureLen];
            rv = _p11.C_Sign(session.SessionId, data, Convert.ToUInt32(data.Length), signature, ref signatureLen);
            if (rv != CKR.CKR_OK)
            {
                throw new Pkcs11Exception("C_Sign", rv);
            }

            if ((ulong)signature.Length != signatureLen)
            {
                Array.Resize(ref signature, Convert.ToInt32(signatureLen));
            }

            return signature;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool.LowLevelExtensions
{
    internal static class SecurityUtils
    {
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public unsafe static void ExecuteUsingSecureUtf8String(SecureString secureString, Action<byte[]> action)
        {
            IntPtr secureStringBstr = IntPtr.Zero;
            IntPtr utf8Buffer = IntPtr.Zero;
            int maxUtf8BytesCount = 0;

            try
            {
                secureStringBstr = Marshal.SecureStringToBSTR(secureString);
                maxUtf8BytesCount = Encoding.UTF8.GetMaxByteCount(secureString.Length);
                utf8Buffer = Marshal.AllocHGlobal(maxUtf8BytesCount);

                char* utf16CharsPtr = (char*)secureStringBstr.ToPointer();
                byte* utf8BytesPtr = (byte*)utf8Buffer.ToPointer();

                int utf8BytesCount = Encoding.UTF8.GetBytes(utf16CharsPtr, secureString.Length, utf8BytesPtr, maxUtf8BytesCount);

                Marshal.ZeroFreeBSTR(secureStringBstr);
                secureStringBstr = IntPtr.Zero;

                byte[] utf8Bytes = new byte[utf8BytesCount];
                GCHandle utf8BytesPin = GCHandle.Alloc(utf8Bytes, GCHandleType.Pinned);
                Marshal.Copy(utf8Buffer, utf8Bytes, 0, utf8BytesCount);
                ClearBuffer(utf8BytesPtr, maxUtf8BytesCount);
                Marshal.FreeHGlobal(utf8Buffer);
                utf8Buffer = IntPtr.Zero;

                try
                {
                    action.Invoke(utf8Bytes);
                }
                finally
                {
                    ClearBuffer(utf8Bytes);
                    utf8BytesPin.Free();
                }
            }
            finally
            {
                if (secureStringBstr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(secureStringBstr);
                }

                if (utf8Buffer != IntPtr.Zero && maxUtf8BytesCount != 0)
                {
                    ClearBuffer((byte*)utf8Buffer.ToPointer(), maxUtf8BytesCount);
                    Marshal.FreeHGlobal(utf8Buffer);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void ClearBuffer(byte[] pin)
        {
            if (pin is null)
            {
                return;
            }

            for (int i = 0; i < pin.Length; i++)
            {
                pin[i] = 0x00;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private unsafe static void ClearBuffer(byte* buffer, int lenght)
        {
            for (int i = 0; i < lenght; i++)
            {
                buffer[i] = 0x00;
            }
        }
    }
}

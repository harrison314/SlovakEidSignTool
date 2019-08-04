using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SlovakEidSignTool
{
    internal static class SecurityUtils
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SafeClearPin(byte[] pin)
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
    }
}

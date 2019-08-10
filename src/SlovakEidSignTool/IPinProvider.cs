using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace SlovakEidSignTool
{
    /// <summary>
    /// PIN provider.
    /// </summary>
    public interface IPinProvider
    {
        /// <summary>
        /// Gets the BOK PIN from user.
        /// </summary>
        /// <returns>User BOK PIN.</returns>
        SecureString GetBokPin();

        /// <summary>
        /// Gets the ZEP PIN from user.
        /// </summary>
        /// <returns>User ZEP PIN.</returns>
        SecureString GetZepPin();
    }
}

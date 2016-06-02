//----------------------------------------------------------------------------- 
// <copyright file=SecureStringExtentions">
//   Copyright (c) Frederic Thevenet. All Rights Reserved.
// </copyright>
// <author>Frederic Thevenet</author>
// <license>
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// </license>
//----------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using log4net;

namespace Eclair.Tools
{
    /// <summary>
    /// Defines static methods to encrypt, decrypt and convert to <see cref="System.String"/> to <see cref="System.Security.SecureString"/>
    /// </summary>
    public static class SecureStringExtentions
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SecureStringExtentions));
        private static readonly byte[] entropy = Encoding.Unicode.GetBytes("arMaZvLnhLwnsRC6BlbekhUX1KyBM0uZIZ8xB00o8amexR5cmnppcH1OleK61kIln5SdK3tdT45LuF9q");
      
        /// <summary>
        /// Protects the string so that it can be securely stored in a configuration file.
        /// </summary>
        /// <param name="input">The string to encrypt.</param>
        /// <returns>the encrypted string.</returns>
        public static string Protect(this string input)
        {
            return Convert.ToBase64String(
                ProtectedData.Protect(
                     Encoding.Unicode.GetBytes(input),
                     entropy,
                     DataProtectionScope.CurrentUser));
        }

        /// <summary>
        /// Decrypts the protected data.
        /// </summary>
        /// <param name="encryptedData">The data to decrypt.</param>
        /// <returns>The decrypted string.</returns>
        public static string Unprotect(this string encryptedData)
        {
            try
            {
                return Encoding.Unicode.GetString(
                    ProtectedData.Unprotect(
                         Convert.FromBase64String(encryptedData),
                         entropy,
                         DataProtectionScope.CurrentUser));
            }
            catch (CryptographicException cex)
            {
                logger.Warn("Error decrypting string", cex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Converts the string to a <see cref="System.Security.SecureString"/>
        /// </summary>
        /// <param name="input">The string to secure.</param>
        /// <returns>the resulting <see cref="System.Security.SecureString"/>.</returns>
        public static SecureString ToSecureString(this string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        /// <summary>
        /// Converts the <see cref="System.Security.SecureString"/> to a normal string.
        /// </summary>
        /// <param name="input">The <see cref="System.Security.SecureString"/> to convert.</param>
        /// <returns>A string that represent the content of the provided <see cref="System.Security.SecureString"/>.</returns>
        public static string ToUnsecureString(this SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
    }
}

//----------------------------------------------------------------------------- 
// <copyright file=CIEqualityComparer">
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
using System.Text;
using System.Security.Cryptography;
using System.Globalization;

namespace Eclair.Tools
{

    /// <summary>
    /// Provides an implementation of IEqualityComparer for strings that is insensitive to case.
    /// </summary>
    public class CiEqualityComparer
            : EqualityComparer<string>
    {
        /// <summary>
        /// Determines whether the two specified strings are equal in case insensitive fashion.
        /// </summary>        
        /// <param name="x">The fist string to compare.</param>
        /// <param name="y">the second string to compare.</param>
        /// <returns>True is the two strings are equal, false otherwise.</returns>
        public override bool Equals(string x, string y)
        {
            if (x == null)
                throw new ArgumentNullException("x");

            if (y == null)
                throw new ArgumentNullException("y");

            return x.Equals(y, StringComparison.OrdinalIgnoreCase);
        }
    
        /// <summary>
        /// Returns the hash code for this string.
        /// </summary>
        /// <remarks>The same hash is returned regardless of the case of the string.</remarks>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override int GetHashCode(string obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return obj.ToUpper(CultureInfo.InvariantCulture).GetHashCode();            
        }
    }
}

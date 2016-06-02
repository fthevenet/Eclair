//----------------------------------------------------------------------------- 
// <copyright file=StreamTools">
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
using System.IO;

namespace Eclair.Tools
{

    /// <summary>
    /// Utilities to copy the whole content of a stream into another one.
    /// </summary>
    /// <remarks>
    /// Starting with version 4.0, the .NET Framework includes build-in methods for this purpose, which should be used instead.
    /// </remarks>
    public static class StreamExtentions
    {
        /// <summary>
        /// Copies bytes from the current stream and writes them to the destination stream.
        /// </summary>
        /// <param name="source">The stream to read from.</param>
        /// <param name="destination">The stream to copy to.</param>
        /// <returns></returns>
        [Obsolete("If running FrameWork 4.0 or higher, use the built-in Stream.CopyTo instead")]
        public static long CopyTo(this Stream source, Stream destination)
        {
            return CopyTo(source, destination, 8192);
        }

        /// <summary>
        /// Copies bytes from the current stream and writes them to the destination stream using a specified buffer size.
        /// </summary>
        /// <param name="source">The stream to read from.</param>
        /// <param name="destination">The stream to copy to.</param>
        /// <param name="bufferSize">The size of the buffer.</param>
        /// <returns></returns>
        [Obsolete("If running FrameWork 4.0 or higher, use the built-in Stream.CopyTo instead")]
        public static long CopyTo(this Stream source, Stream destination, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int byteRead = 0;
            long byteTotal = 0;
            while ((byteRead = source.Read(buffer, 0, bufferSize)) > 0)
            {
                destination.Write(buffer, 0, byteRead);
                byteTotal += byteRead;
            }
            return byteTotal;
        }

    }
}

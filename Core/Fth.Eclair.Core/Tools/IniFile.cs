//----------------------------------------------------------------------------- 
// <copyright file=IniFile">
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

namespace Eclair.Tools
{
    /// <summary>
    /// Provides static methods to manipulate INI files.
    /// </summary>
    public class IniFile
    {
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);
       
        /// <summary>
        /// Writes the specified value to the key in the section specified.
        /// </summary>
        /// <param name="path">The path to the ini file</param>
        /// <param name="section">The section that contains the key to update</param>
        /// <param name="key">The name of</param>
        /// <param name="value"></param>
        public static void WriteValue(string path, string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, path);
        }

        /// <summary>
        /// Reads the value of the specified key in an ini file.
        /// </summary>
        /// <param name="path">The path to the ini file.</param>
        /// <param name="section">The section of the key.</param>
        /// <param name="key">the key to return the value for.</param>
        /// <returns>The value of the specified key.</returns>
        public static string ReadValue(string path, string section, string key)
        {
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "",  temp, 255, path);
            return temp.ToString();
        }

        /// <summary>
        /// Delete a key  of the specified section.
        /// </summary>
        /// <param name="path">The path to the ini file.</param>
        /// <param name="section">The section of the key.</param>
        /// <param name="key">The key to delete.</param>
        public static void DeleteEntry(string path, string section, string key)
        {
             WritePrivateProfileString(section, key, null, path);
        }

        /// <summary>
        /// Deletes a section of an ini file.
        /// </summary>
        /// <param name="path">The path to the ini file.</param>
        /// <param name="section">The section to delete.</param>
        public static void DeleteSection(string path, string section)
        {
             WritePrivateProfileString(section, null, null, path);
        }
    }
}

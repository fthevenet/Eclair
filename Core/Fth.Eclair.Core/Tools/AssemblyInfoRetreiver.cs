//----------------------------------------------------------------------------- 
// <copyright file=AssemblyInfoRetreiver">
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
using System.Reflection;
using System.IO;

namespace Eclair.Tools
{
    /// <summary>
    /// Provides utilities to retrieve information about assemblies.
    /// </summary>
    public class AssemblyInfoRetriever
    {
        private static volatile AssemblyInfoRetriever instance;
        private static object syncRoot = new object();
        private Assembly asm;

        /// <summary>
        /// Gets a singleton instance of the AssemblyInfoRetreiver class.
        /// </summary>
        /// <remarks>
        /// The singleton instance can retrieve info about the assembly containing this type.
        /// </remarks>
        public static AssemblyInfoRetriever DefaultInstance
        {
            get
            {
                lock (syncRoot)
                {
                    if (AssemblyInfoRetriever.instance == null)
                        AssemblyInfoRetriever.instance = new AssemblyInfoRetriever();
                }
                return instance;
            }
        }

        private AssemblyInfoRetriever()
        {
            this.asm = this.GetType().Assembly;
        }

        /// <summary>
        /// Initializes a new instance of the AssemblyInfoRetreiver class for the specified assembly.
        /// </summary>
        /// <param name="asm">The assembly to retrieve info from.</param>
        /// <remarks>
        /// An instance created through this constructor can retrieve info about the specified assembly. 
        /// </remarks>
        public AssemblyInfoRetriever(Assembly asm)
        {
            this.asm = asm;
        }

        /// <summary>
        /// Initializes a new instance of the AssemblyInfoRetreiver class for the assembly loaded from the specified path.
        /// </summary>
        /// <param name="asmPath">The path where the assembly to load is located.</param>
        /// <remarks>
        /// An instance created through this constructor can retrieve info about the specified assembly. 
        /// </remarks>
        public AssemblyInfoRetriever(string  asmPath)
        {
            this.asm = Assembly.LoadFile(asmPath);
        }

        /// <summary>
        /// Gets the location of the assembly as specified originally, for example, in an AssemblyName object.
        /// </summary>
        public string FileName
        {
            get { return Path.GetFileName(this.asm.CodeBase); }
        }

        /// <summary>
        /// Gets the full path or UNC location of the loaded file that contains the manifest.
        /// </summary>
        public string Location
        {
            get { return this.asm.Location; }
        }


        /// <summary>
        /// Gets the assembly title information.
        /// </summary>
        /// <remarks>
        /// If the AssemblyTitle attribute is not specified for the assembly, the full name of the assembly is returned. 
        /// </remarks>
        public string Title
        {
            get
            {              
                try
                {
                    return ((AssemblyTitleAttribute)this.asm.GetCustomAttributes(Type.GetType("System.Reflection.AssemblyTitleAttribute"), false)[0]).Title;
                }
                catch
                {
                    return this.asm.FullName;
                }
            }
        }

        /// <summary>
        /// Gets the assembly copyright information.
        /// </summary>
        /// <remarks>
        /// If the AssemblyCopyright attribute is not specified for the assembly, an empty string is returned. 
        /// </remarks>
        public string Copyright
        {

            get
            {
                object[] copyAttributes = this.asm.GetCustomAttributes(Type.GetType("System.Reflection.AssemblyCopyrightAttribute"), false);
                if (copyAttributes.Length > 0)
                    return ((AssemblyCopyrightAttribute)copyAttributes[0]).Copyright;

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the assembly description information.
        /// </summary>
        /// <remarks>
        /// If the  AssemblyDescription attribute is not specified for the assembly, an empty string is returned. 
        /// </remarks>
        public string Description
        {
            get
            {                
                try
                {
                    return ((AssemblyDescriptionAttribute)this.asm.GetCustomAttributes(Type.GetType("System.Reflection.AssemblyDescriptionAttribute"), false)[0]).Description;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the assembly file version information.
        /// </summary>
        /// <remarks>
        /// If the AssemblyFileVersion attribute is not specified for the assembly, new Version(0, 0, 0, 0) is returned. 
        /// </remarks>
        public Version FileVersion
        {
            get
            {
                try
                {
                    return new Version(((AssemblyFileVersionAttribute)this.asm.GetCustomAttributes(Type.GetType("System.Reflection.AssemblyFileVersionAttribute"), false)[0]).Version);
                }
                catch
                {
                    return new Version(0, 0, 0, 0);
                }
            }
        }

        /// <summary>
        /// Gets the major, minor, build, and revision numbers of the assembly. 
        /// </summary>
        public Version Version
        {
            get
            {
                return this.asm.GetName().Version;           
            }
        }

    }
}

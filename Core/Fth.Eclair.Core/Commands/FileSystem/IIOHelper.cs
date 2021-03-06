﻿//----------------------------------------------------------------------------- 
// <copyright file=IIOHelper">
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
using System.IO;

namespace Eclair.Commands.FileSystem
{
    
    /// <summary>
    /// Defines methods to abstract manipulations of paths, files and directories.
    /// </summary>
    /// <remarks>
    /// The purpose of this interface is to abstract IO operations so that they can be handled through APIs that supports path longer than 
    /// 260 characters.
    /// </remarks>
    public interface IIOHelper
    {
        /// <summary>
        /// Gets a value indicating whether the specified path string contains a root.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>true if path contains a root; otherwise, false.</returns>
        bool IsPathRooted(string path);

        /// <summary>
        /// Changes the extension of a path string.
        /// </summary>
        /// <param name="path">The path information to modify.</param>
        /// <param name="extention">The new extension (with or without a leading period).</param>
        /// <returns>The modified path information.</returns>
        string ChangeExtension(string path, string extention);

        /// <summary>
        /// Returns the directory information for the specified path string.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns>
        /// Directory information for path, or null if path denotes a root directory
        /// or is null. Returns System.String.Empty if path does not contain directory information.
        /// </returns>
        string GetDirectoryName(string path);


        /// <summary>
        ///  Returns the file name and extension of the specified path string.
        /// </summary>
        /// <param name="path">The path string from which to obtain the file name and extension.</param>
        /// <returns> The characters after the last directory character in path.</returns>
        string GetFileName(string path);

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>true if path refers to an existing directory; otherwise, false.</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Deletes an empty directory from a specified path.
        /// </summary>
        /// <param name="path">The name of the empty directory to remove. </param>
        void DirectoryDelete(string path);

        /// <summary>
        /// Creates all directories and subdirectories in the specified path.
        /// </summary>
        /// <param name="path">The directory path to create.</param>
        void DirectoryCreate(string path);

        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path, 
        /// and searches in all its sub folders.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter can contain
        /// a combination of valid literal path and wildcard (* and ?) characters, but doesn't support regular expressions.
        /// </param>
        /// <param name="errorHandler">The delegate that is invoked when a non fatal exception is thrown.</param>
        /// <returns>
        /// An enumerable collection of the full names (including paths) for the files in the directory
        /// specified by path and that match the specified search pattern and option. 
        /// </returns>
        IEnumerable<string> EnumerateFiles(string path, string searchPattern, HandleNonFatalException errorHandler);

        /// <summary>
        ///  Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>true if the caller has the required permissions and path contains the name of an existing file; otherwise, false. 
        ///  </returns>
        bool FileExists(string path);

        /// <summary>
        ///  Deletes the specified file.
        /// </summary>
        /// <param name="path"> The name of the file to be deleted. Wildcard characters are not supported.</param>
        void FileDelete(string path);

        /// <summary>
        /// Opens a System.IO.FileStream on the specified path, having the specified
        /// mode with read, write, or read/write access and the specified sharing option.       
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode"> 
        /// A System.IO.FileMode value that specifies whether a file is created if one
        /// does not exist, and determines whether the contents of existing files are
        /// retained or overwritten.
        /// </param>
        /// <param name="access">A System.IO.FileAccess value that specifies the operations that can be performed on the file.</param>
        /// <param name="share">A System.IO.FileShare value specifying the type of access other threads have to the file.</param>
        /// <returns>A System.IO.FileStream on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</returns>
        FileStream FileOpen(string path, FileMode mode, FileAccess access, FileShare share);

        ///<summary>
        ///  Returns a list of paths from the specified string enumerable.
        /// </summary>
        /// <remarks>Supports path longer than 260 characters.</remarks>
        /// <param name="cmdLines">The IEnumerable&lt;string&gt; to retrieve paths from.</param>
        /// <returns>An IEnumerable&lt;string&gt; of all retrieved path.</returns>
        IEnumerable<string> GetAllPathFromArguments(IEnumerable<string> cmdLines);

    }
}

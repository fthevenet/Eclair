//----------------------------------------------------------------------------- 
// <copyright file=FailSafeFileBrowser">
//   Copyright (c) Frederic Thevenet. All Rights Reserved
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
using System.Threading.Tasks;
using System.IO;
using System.Security;
using log4net;
using Microsoft.Experimental.IO;

namespace Eclair.Commands.FileSystem
{
    /// <summary>
    /// Delegate for handling non fatal exception thrown during the execution of a command. 
    /// </summary>
    /// <param name="exception">The exception to handle</param>
    /// <param name="message">The message to log.</param>
    public delegate void HandleNonFatalException(Exception exception, string message);

    /// <summary>
    /// Provides static methods to enumerate or return a list of all files in a folder and its sub folder recursively, 
    /// without getting interrupted when UnauthorizedAccessException, PathTooLongException, DirectoryNotFoundException or
    /// SecurityException are thrown while browsing through files and folders.
    /// </summary>
    /// <remarks>
    /// When an exception is thrown while trying to access a file or folder, it is handled internally and does not interrupt
    /// the enumeration. The file that caused the exception will not be returned. In case the exception was thrown while trying
    /// to access a folder, files contained in that folder structure will be ignored.
    /// </remarks>
    public static class FailSafeFileBrowser
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(FailSafeFileBrowser));
   
        #region Public static methods
        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path, 
        ///  and searches in all its sub folders.
        /// </summary>
        /// <remarks>
        /// When a non fatal exception is thrown, it is handled by being logged as a warning to
        /// the Log4net logger for the class FailSafeFileBrowser. 
        /// </remarks>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter can contain
        /// a combination of valid literal path and wild card (* and ?) characters, but doesn't support regular expressions.
        /// </param>
        /// <returns>
        /// An enumerable collection of the full names (including paths) for the files in the directory
        /// specified by path and that match the specified search pattern and option. 
        /// </returns>
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return EnumerateFiles(path, searchPattern, (e, m) => logger.Warn(m, e));
        }

        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path, 
        /// and searches in all its sub folders.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter can contain
        /// a combination of valid literal path and wild card (* and ?) characters, but doesn't support regular expressions.
        /// </param>
        /// <param name="errorHandler">The delegate that is invoked when a non fatal exception is thrown.</param>
        /// <returns>
        /// An enumerable collection of the full names (including paths) for the files in the directory
        /// specified by path and that match the specified search pattern and option. 
        /// </returns>
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, HandleNonFatalException errorHandler)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("currentDir");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(String.Format("The path {0} is does not point to a valid directory", path));

            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException("searchPattern");

            if (errorHandler == null)
                throw new ArgumentNullException("errorHandler");

            return enumerateFiles(path, searchPattern, errorHandler);
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path, 
        ///  and searches in all its sub folders.
        /// <para>
        /// This methods is capable of enumeratingFiles files with path longer than 260 characters, up to 32000 character.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Because it is capable of enumeratingFiles files with paths longer than, the code using this method must be able to handle long paths.
        /// </para>
        /// <para>
        /// When a non fatal exception is thrown, it is handled by being logged as a warning to
        /// the Log4net logger for the class FailSafeFileBrowser. 
        /// </para>
        /// </remarks>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter can contain
        /// a combination of valid literal path and wild card (* and ?) characters, but doesn't support regular expressions.
        /// </param>
        /// <returns>
        /// An enumerable collection of the full names (including paths) for the files in the directory
        /// specified by path and that match the specified search pattern and option. 
        /// </returns>
        public static IEnumerable<string> LongPathEnumerateFiles(string path, string searchPattern)
        {
            return LongPathEnumerateFiles(path, searchPattern, (e, m) => logger.Warn(m, e));
        }


        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path, 
        /// and searches in all its sub folders.
        /// <para>
        /// This methods is capable of enumeratingFiles files with path longer than 260 characters, up to 32000 character.
        /// </para>
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter can contain
        /// a combination of valid literal path and wild card (* and ?) characters, but doesn't support regular expressions.
        /// </param>
        /// <param name="errorHandler">The delegate that is invoked when a non fatal exception is thrown.</param>
        /// <returns>
        /// An enumerable collection of the full names (including paths) for the files in the directory
        /// specified by path and that match the specified search pattern and option. 
        /// </returns>
        public static IEnumerable<string> LongPathEnumerateFiles(string path, string searchPattern, HandleNonFatalException errorHandler)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("currentDir");

            if (!LongPathDirectory.Exists(path))
                throw new DirectoryNotFoundException(String.Format("The path {0} is does not point to a valid directory", path));

            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException("searchPattern");

            if (errorHandler == null)
                throw new ArgumentNullException("errorHandler");

            return longPathEnumerateFiles(path, searchPattern, errorHandler);
        }

        /// <summary>
        /// Returns an array of file names that match a search pattern in a specified path, and searches in all its sub folders.
        /// </summary>
        /// <remarks>
        /// When a non fatal exception is thrown, it is handled by being logged as a warning to
        /// the Log4net logger for the class FailSafeFileBrowser. 
        /// </remarks>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter can contain
        /// a combination of valid literal path and wild card (* and ?) characters, but doesn't support regular expressions.
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the directory
        /// specified by path and that match the specified search pattern and option. 
        /// </returns>
        public static string[] GetFiles(string path, string searchPattern)
        {
            return GetFiles(path, searchPattern, (e, m) => logger.Warn(m, e));
        }

        /// <summary>
        /// Returns an array of file names that match a search pattern in a specified path, and searches in all its sub folders.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter can contain
        /// a combination of valid literal path and wild card (* and ?) characters, but doesn't support regular expressions.
        /// </param>
        /// <param name="errorHandler">The delegate that is invoked when a non fatal exception is thrown.</param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the directory
        /// specified by path and that match the specified search pattern and option. 
        /// </returns>
        public static string[] GetFiles(string path, string searchPattern, HandleNonFatalException errorHandler)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("currentDir");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(String.Format("The path {0} is does not point to a valid directory.", path));

            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException("searchPattern");

            if (errorHandler == null)
                throw new ArgumentNullException("errorHandler");

            var files = new List<string>();
            getFiles(files, path, searchPattern, errorHandler);

            return files.ToArray();
        }


        /// <summary>
        /// Determines whether the specified path matches the specified search pattern string. 
        /// A search pattern can contain '*' and '?' wildcards, i.e. *.txt, *.*, c:\h?llo.txt, etc...
        /// </summary>
        /// <param name="searchPattern">The search pattern to match.</param>
        /// <param name="path">The path to compare.</param>
        /// <param name="caseSensitive">Set to true if the comparison should be case sensitive, false otherwise.</param>
        /// <returns>True if the provided path matches the provided search pattern, false otherwise.</returns>
        public static bool PathMatchesSearchPattern(string searchPattern, string path, bool caseSensitive)
        {
            return pathMatchesSearchPattern(searchPattern, path, caseSensitive);
        }

        #endregion

        #region Private members
        private static IEnumerable<string> enumerateFiles(string currentDir, string searchPattern, HandleNonFatalException errorHandler)
        {           
            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(currentDir, searchPattern);
            }
            catch (DirectoryNotFoundException ex)
            {
                errorHandler(ex, string.Format("An error occurred while retrieving files list from folder {0} with pattern {1}.", currentDir, searchPattern));
                yield break;
            }
            catch (SecurityException ex)
            {
                errorHandler(ex, string.Format("An error occurred while retrieving files list from folder {0} with pattern {1}.", currentDir, searchPattern));
                yield break;
            }
            catch (UnauthorizedAccessException ex)
            {
                errorHandler(ex, string.Format("An error occurred while retrieving files list from folder {0} with pattern {1}.", currentDir, searchPattern));
                yield break;
            }
            catch (PathTooLongException ex)
            {
                errorHandler(ex, string.Format("An error occurred while retrieving files list from folder {0} with pattern {1}.", currentDir, searchPattern));
                yield break;
            }

            foreach (string file in files)
            {
                if (logger.IsDebugEnabled)
                    logger.DebugFormat("Enumerating {0}", file);
            
                yield return file;
            }

            IEnumerable<string> subDirectories;
            try
            {
                subDirectories = Directory.EnumerateDirectories(currentDir);
            }
            catch (DirectoryNotFoundException ex)
            {
                errorHandler(ex, string.Format("An error occurred while retrieving folder list from folder {0}", currentDir));
                yield break;
            }
            catch (SecurityException ex)
            {
                errorHandler(ex, string.Format("An error occurred while retrieving folder list from folder {0}", currentDir));
                yield break;
            }
            catch (UnauthorizedAccessException ex)
            {
                errorHandler(ex, string.Format("An error occurred while retrieving folder list from folder {0}", currentDir));
                yield break;
            }
            catch (PathTooLongException ex)
            {
                errorHandler(ex, string.Format("An error occurred while retrieving folder list from folder {0}", currentDir));
                yield break;
            }

            foreach (string file in subDirectories.SelectMany(subDir => enumerateFiles(subDir, searchPattern, errorHandler))) 
                yield return file;
        }

        private static IEnumerable<string> longPathEnumerateFiles(string currentDir, string searchPattern, HandleNonFatalException errorHandler)
       { 

            var fileEnumerator = LongPathDirectory.EnumerateFiles(currentDir, searchPattern).GetEnumerator();
            bool enumeratingFiles = true;
            while (enumeratingFiles)
            {
                try
                {
                    enumeratingFiles = fileEnumerator.MoveNext();
                }
                catch (DirectoryNotFoundException ex)
                {
                    errorHandler(ex, string.Format("An error occurred while retrieving files list from folder {0} with pattern {1}.", currentDir, searchPattern));
                    enumeratingFiles = false;
                    yield break;
                }
                catch (SecurityException ex)
                {
                    errorHandler(ex, string.Format("An error occurred while retrieving files list from folder {0} with pattern {1}.", currentDir, searchPattern));
                    enumeratingFiles = false;
                    yield break;
                }
                catch (UnauthorizedAccessException ex)
                {
                    errorHandler(ex, string.Format("An error occurred while retrieving files list from folder {0} with pattern {1}.", currentDir, searchPattern));
                    enumeratingFiles = false;
                    yield break;
                }
                catch (PathTooLongException ex)
                {
                    errorHandler(ex, string.Format("An error occurred while retrieving files list from folder {0} with pattern {1}.", currentDir, searchPattern));
                    enumeratingFiles = false;
                    yield break;
                }

                if (enumeratingFiles)
                {
                    if (logger.IsDebugEnabled)
                        logger.DebugFormat("Enumerating {0}", fileEnumerator.Current);

                    yield return fileEnumerator.Current;
                }
            }

            var subDirectoriesEnumerator = LongPathDirectory.EnumerateDirectories(currentDir).GetEnumerator();
            bool enumeratingDirectories = true;
            while (enumeratingDirectories)
            {
                try
                {
                    enumeratingDirectories = subDirectoriesEnumerator.MoveNext();
                }
                catch (DirectoryNotFoundException ex)
                {
                    errorHandler(ex, string.Format("An error occurred while retrieving folder list from folder {0}", currentDir));
                    enumeratingDirectories = false;
                    yield break;
                }
                catch (SecurityException ex)
                {
                    errorHandler(ex, string.Format("An error occurred while retrieving folder list from folder {0}", currentDir));
                    enumeratingDirectories = false;
                    yield break;
                }
                catch (UnauthorizedAccessException ex)
                {
                    errorHandler(ex, string.Format("An error occured while retreiving folder list from folder {0}", currentDir));
                    enumeratingDirectories = false;
                    yield break;
                }
                catch (PathTooLongException ex)
                {
                    errorHandler(ex, string.Format("An error occured while retreiving folder list from folder {0}", currentDir));
                    enumeratingDirectories = false;
                    yield break;
                }
                if (enumeratingDirectories)
                {
                    foreach (var file in longPathEnumerateFiles(subDirectoriesEnumerator.Current, searchPattern, errorHandler))
                        yield return file;
                }
            }
        }

        private static void getFiles(List<string> files, string currentDir, string searchPattern, HandleNonFatalException errorHandler)
        {
            try
            {
                files.AddRange(Directory.GetFiles(currentDir, searchPattern));
            }
            catch (DirectoryNotFoundException ex)
            {
                errorHandler(ex, "An error occured while retreiving files list.");
            }
            catch (SecurityException ex)
            {
                errorHandler(ex, "An error occured while retreiving files list.");
            }
            catch (UnauthorizedAccessException ex)
            {
                errorHandler(ex, "An error occured while retreiving folder list.");
            }
            catch (PathTooLongException ex)
            {
                errorHandler(ex, "An error occured while retreiving folders list.");
            }

            try
            {
                foreach (var subDir in Directory.GetDirectories(currentDir))
                {
                    getFiles(files, subDir, searchPattern, errorHandler);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                errorHandler(ex, "An error occured while retreiving files list.");
            }
            catch (SecurityException ex)
            {
                errorHandler(ex, "An error occured while retreiving files list.");
            }
            catch (UnauthorizedAccessException ex)
            {
                errorHandler(ex, "An error occured while retreiving folder list.");
            }
            catch (PathTooLongException ex)
            {
                errorHandler(ex, "An error occured while retreiving folders list.");
            }
        }

        private static unsafe bool pathMatchesSearchPattern(string path,string searchPattern, bool caseSensitive)
        {
            if (!caseSensitive)
            {
                searchPattern = searchPattern.ToUpper();
                path = path.ToUpper();
            }

            fixed (char* pPattern = searchPattern, pPath = path)
            {
                int pPatternInc = 0;
                int pPathInc = 0;

                int mp = 0;
                int cp = 0;

                while ((*(pPath + pPathInc) != 0) && (*(pPattern + pPatternInc) != '*'))
                {
                    if ((*(pPattern + pPatternInc) != *(pPath + pPathInc)) && (*(pPattern + pPatternInc) != '?'))
                        return false;

                    pPatternInc++;
                    pPathInc++;
                }

                while (*(pPath + pPathInc) != 0)
                {
                    if (*(pPattern + pPatternInc) == '*')
                    {
                        if (0 == *(pPattern + ++pPatternInc))
                            return true;

                        mp = pPatternInc;
                        cp = pPathInc + 1;
                    }
                    else if ((*(pPattern + pPatternInc) == *(pPath + pPathInc)) || (*(pPattern + pPatternInc) == '?'))
                    {
                        pPatternInc++;
                        pPathInc++;
                    }
                    else
                    {
                        pPatternInc = mp;
                        pPathInc = cp++;
                    }
                }

                while (*(pPattern + pPatternInc) == '*')
                {
                    pPatternInc++;
                }
                return (0 == *(pPattern + pPatternInc));
            }
        } 
        #endregion
    }
}

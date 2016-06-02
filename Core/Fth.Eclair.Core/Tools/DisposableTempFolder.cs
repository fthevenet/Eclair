//----------------------------------------------------------------------------- 
// <copyright file=DisposableTempFolder">
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
using System.IO;
using System.Collections.Generic;


namespace Eclair.Tools
{

    
    /// <summary>
    /// The exception that is thrown when an error occurs in an instance of the DisposableTempFolder class.
    /// </summary>
    public class DisposableTempFolderException : Exception
    {

        /// <summary>
        /// Initializes a new instance of the DisposableTempFolderException class with a specified error message. 
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public DisposableTempFolderException(string message):base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DisposableTempFolderException class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public DisposableTempFolderException(string message, Exception innerException):base(message,innerException)
        {
        }

    }


    /// <summary>
    /// Represent a disposable collection of DisposableTempFolder objects. All items of the collections are disposed when the instance of the collection is disposed,
    /// </summary>
    public class DisposableTempFolderCollection: List<DisposableTempFolder>, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the DisposableTempFolder class and adds it to the collection.
        /// </summary>
        /// <returns>
        /// The newly created DisposableTempFolder object.
        /// </returns>
        public DisposableTempFolder CreateOne()
        {
            DisposableTempFolder tmp = new DisposableTempFolder();
            this.Add(tmp);
            return tmp;
        }

        /// <summary>
        /// Initializes a new instance of the DisposableTempFolder in the specified root folder class and adds it to the collection.
        /// </summary>
        /// <param name="tempFolderRoot">The root folder in which the temp folder will be created.</param>
        /// <returns>
        /// The newly created DisposableTempFolder object.
        /// </returns>
        public DisposableTempFolder CreateOne(string tempFolderRoot)
        {
            DisposableTempFolder tmp = new DisposableTempFolder(tempFolderRoot);
            this.Add(tmp);
            return tmp;
        }

        #region Membres de IDisposable


        private bool disposed;

        /// <summary>
        /// Dispose the collection.
        /// </summary>
        /// <remarks>
        /// All DisposableTempFolder objects in the collection are disposed.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose the collection. 
        /// </summary>
        /// <param name="disposing">True if managed ressources should be disposed of, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    foreach (DisposableTempFolder tmp in this)
                    {
                        tmp.Dispose();
                    }
                }
            }
            disposed = true;
        }

        /// <summary>
        /// Destroys the current DisposableTempFolderCollection instance.
        /// </summary>
        ~DisposableTempFolderCollection()
        {
            Dispose(false);
        }

        #endregion

    }

    /// <summary>
    /// Represent a temporary folder that will be deleted along with its content when the instance is disposed.
    /// </summary>
    public class DisposableTempFolder : IDisposable 	
    {
        private DirectoryInfo dirInfo;
        private string fullPathStr;

        /// <summary>
        ///  Gets the name of the disposable folder.
        /// </summary>
        public string Name
        {
            get { return this.dirInfo.Name;}
        }

        /// <summary>
        /// Gets the full path of the disposable folder.
        /// </summary>
        public string FullName
        {
            get { return this.dirInfo.FullName;}
        }

        /// <summary>
        /// Gets the root portion of a path.
        /// </summary>
        public DirectoryInfo Root
        {
            get { return this.dirInfo.Root;}
        }

        /// <summary>
        /// Gets the parent directory of a specified subdirectory.
        /// </summary>
        public DirectoryInfo Parent
        {
            get { return this.dirInfo.Parent;}
        }

        /// <summary>
        /// Gets the temporay folder's extention.
        /// </summary>
        public string Extension
        {
            get { return this.dirInfo.Extension; }
        }

        /// <summary>
        /// Initializes a new instance of the DisposableTempFolder class with a custom root directory. 
        /// </summary>
        /// <param name="rootDir">The path to the root of the disposable folder.</param>
        public DisposableTempFolder(string rootDir)
        {            
                this.initTmpFolder(rootDir);
        }

        /// <summary>
        /// Initializes a new instance of the DisposableTempFolder class
        /// </summary>
        public DisposableTempFolder()
        {
            this.initTmpFolder(Path.GetTempPath());       
        }

        /// <summary>
        /// Gets a random file name based inside the temporary folder.
        /// </summary>
        /// <returns>A FileInfo object to the new temp name.</returns>
        public FileInfo GetRandomFilePath()
        {
            string tmp;
            do
            {
                tmp = Path.Combine(this.FullName, Path.GetRandomFileName());
            }
            while (File.Exists(tmp));

            return new FileInfo(tmp);

        }

        /// <summary>
        /// Returns a file list from the current disposable folder. 
        /// </summary>
        /// <returns>An array of propertyType FileInfo.</returns>
        public FileInfo[] GetFiles()
        {
            return this.dirInfo.GetFiles();
        }

        /// <summary>
        /// Returns a file list from the current disposable folder matching the given searchPattern.
        /// </summary>
        /// <param name="searchPattern">The search string, such as "*.txt".</param>
        /// <returns>An array of propertyType FileInfo.</returns>
        public FileInfo[] GetFiles(string searchPattern)
        {
            return this.dirInfo.GetFiles(searchPattern);
        }   

        /// <summary>
        ///  Returns the subdirectories of the current disposable folder.
        /// </summary>
        /// <returns>An array of propertyType DirectoryInfo.</returns>
        public DirectoryInfo[] GetDirectories()
        {
            return this.dirInfo.GetDirectories();
        }

        /// <summary>
        /// Returns an array of directories in the current disposable folder matching the given search criteria.
        /// </summary>
        /// <param name="searchPattern">The search string, such as "System*", used to search for all directories beginning with the word "System".</param>
        /// <returns>An array of propertyType DirectoryInfo matching searchPattern.</returns>
        public DirectoryInfo[] GetDirectories(string searchPattern)
        {
            return this.dirInfo.GetDirectories(searchPattern);
        }
        
        /// <summary>
        /// Creates a subdirectory or subdirectories on the specified path. The specified path can be relative to this instance of the disposable folder. 
        /// </summary>
        /// <param name="path">The specified path. This cannot be a different disk volume or Universal Naming Convention (UNC) name. </param>
        /// <returns>The last directory specified in path.</returns>
        public DirectoryInfo CreateSubdirectory(string path)
        {
            return this.dirInfo.CreateSubdirectory(path);
        }

        /// <summary>
        /// Combines the specified file name with the full path of the temp folder.
        /// </summary>
        /// <param name="fileName">The file name to combine.</param>
        /// <returns>The full path of the file.</returns>
        public string Combine(string fileName)
        {
            return Path.Combine(this.FullName, fileName);
        }

        private void initTmpFolder(string rootDir)
        {
            try
            {
                string tmp;
                do
                {
                    tmp = Path.Combine(rootDir, Path.GetRandomFileName());
                }
                while (Directory.Exists(tmp));

                this.dirInfo = new DirectoryInfo(tmp);
                this.dirInfo.Create();
                this.fullPathStr = tmp; //Store full path as string full deletion w/o relying on DirInfo
            }
            catch (Exception ex)
            {
                throw new DisposableTempFolderException(String.Format("Error creating temporary folder \"{0}\"", this.dirInfo.FullName), ex);
            }
        }	
      
        #region Membres de IDisposable

        private bool disposed;
        
        /// <summary>
        /// Releases all resources used by the current instance of the DisposableTempFolder class. 
        /// This will also delete the underlying directory and its content.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);	
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the DisposableTempFolder and optionally releases the managed resources.
        /// This will also delete the underlying directory and its content.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposed)
            {

                if(disposing)
                {
                    // Dispose managed resources.

                }
                // Release unmanaged resources. If disposing is false, 				
                // Proceed to deletion without relying on managed objects (DirInfo) so that it can be called safely by the finalizer
                try
                {
                    if (Directory.Exists(this.fullPathStr))
                    {	
                        Directory.Delete(this.fullPathStr, true);
                    }
                }
                catch {} //at least I tried...

            }
            disposed = true;         
        }

        /// <summary>
        /// Distructor for DisposableTempFolder
        /// </summary>
        ~DisposableTempFolder()
        {
            Dispose(false);
        }


        #endregion
    }
}

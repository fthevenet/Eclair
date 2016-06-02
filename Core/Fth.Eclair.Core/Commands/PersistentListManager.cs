//----------------------------------------------------------------------------- 
// <copyright file=PersistentListManager">
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using log4net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;



namespace Eclair.Commands
{
    /// <summary>
    /// Provides methods to create and manage collections that can be stored to disk in order to remain persistent in between executions of a process.
    /// </summary>
    [Serializable]
    public sealed class PersistentListManager
    {
        [NonSerialized]
        private readonly static ILog logger = LogManager.GetLogger(typeof(PersistentListManager));
        [NonSerialized]
        private readonly static object singletonLock = new object();
        [NonSerialized]
        private ReaderWriterLockSlim accessLock;
        private static volatile PersistentListManager instance;
        private static readonly string serializePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Fth\\Eclair\\PersistentListManager.state.xml";
        private Dictionary<string, PersistentList> lists;

        private PersistentListManager()
        {
            accessLock = new ReaderWriterLockSlim();
            this.lists = new Dictionary<string, PersistentList>();
        }

        /// <summary>
        /// Gets the singleton instance of the PersistentListManager class.
        /// </summary>
        static public PersistentListManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (singletonLock)
                    {
                        if (instance == null)
                        {
                            try
                            {
                                if (File.Exists(serializePath))
                                {
                                    instance = PersistentListManager.restore();
                                    instance.accessLock = new ReaderWriterLockSlim();
                                }
                                else
                                {
                                    instance = new PersistentListManager();
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.WarnFormat("Failed to restore Persistent Lists from file {0}: {1}", serializePath, ex.Message);
                                if (logger.IsDebugEnabled)
                                    logger.Debug(ex);

                                instance = new PersistentListManager();
                            }
                        }
                    }
                }
                return instance;
            }
        }

        static private PersistentListManager restore()
        {
            using (XmlReader reader = XmlReader.Create(serializePath))
            {
                var serializer = new DataContractSerializer(typeof(PersistentListManager));
                return (PersistentListManager)serializer.ReadObject(reader);
            }
        }

        /// <summary>
        /// Represent a collection that can be stored to disk in order to remain persistent in between executions of a process.
        /// </summary>
        [Serializable]
        public class PersistentList
        {
            [NonSerialized]
            private object _syncRoot;


            /// <summary>
            /// An object that can be used to synchronize concurrent access to the PersistentList instance's methods and properties.
            /// </summary>
            public object SyncRoot
            {
                get
                {
                    if (this._syncRoot == null)
                        Interlocked.CompareExchange(ref this._syncRoot, new object(), null);

                    return this._syncRoot;
                }
            }

            private Dictionary<string, int> innerList;
            private Dictionary<string, int> currentList;

            /// <summary>
            /// Initializes a new instance of the PersistentList class.
            /// </summary>
            public PersistentList()
            {               
                this.innerList = new Dictionary<string, int>();
                this.currentList = new Dictionary<string, int>();
            }

            /// <summary>
            /// Removes all keys and values from the PersistentList instance.
            /// </summary>
            public void Clear()
            {
                lock (this.SyncRoot)
                {
                    this.innerList.Clear();
                    this.currentList.Clear();
                }
            }

            /// <summary>
            /// Gets the number of items contained in the PersistentList instance.
            /// </summary>
            public int Count
            {
                get
                {
                    lock (this.SyncRoot)
                    {
                        return this.innerList.Count;
                    }
                }
            }

            /// <summary>
            /// Returns the value corresponding to the specified key.
            /// </summary>
            /// <param name="key">The key that identifies the value to return.</param>
            /// <returns>The value corresponding to the specified key.</returns>
            public int this[string key]
            {
                get
                {
                    lock (this.SyncRoot)
                    {
                        return (int)this.innerList[key];
                    }
                }
            }

            /// <summary>
            /// Removes the expired items in the collection.
            /// </summary>
            /// <returns>The number of items removed.</returns>
            public int CleanUp()
            {
                lock (this.SyncRoot)
                {
                    var rmList = innerList.Where(kvp => !currentList.ContainsKey(kvp.Key)).ToList();
                    foreach (var item in rmList)
                        innerList.Remove(item.Key);

                    currentList.Clear();
                    return rmList.Count;
                }
            }

            /// <summary>
            /// Specifies whether of not the collection contains an item for the specified key.
            /// </summary>
            /// <param name="key">The key of the items to check.</param>
            /// <returns>True if the collection contains an item for the specified key, False otherwise.</returns>
            public bool Contains(string key)
            {
                lock (this.SyncRoot)
                {
                    if (currentList.ContainsKey(key))
                        logger.WarnFormat("The current list already contain an item named {0}", key);
                    else
                        currentList.Add(key, 0);

                    if (!innerList.ContainsKey(key))
                    {
                        innerList.Add(key, 0);
                        return false;
                    }
                    else
                    {
                        innerList[key]++;
                        return true;
                    }
                }
            }
        }

        /// <summary>
        /// Flushes all PersistentList instances to persistent storage.
        /// </summary>
        public void Flush()
        {
            accessLock.EnterWriteLock();
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(serializePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(serializePath));

                using (var fs = File.Open(serializePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var writer = new XmlTextWriter(fs, new UTF8Encoding()))
                    {
                        var serializer = new DataContractSerializer(this.GetType());
                        writer.Formatting = Formatting.Indented;
                        serializer.WriteObject(writer, this);
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WarnFormat("Failed to flush Persistent Lists to file {0}: {1}", serializePath, ex.Message);
                if (logger.IsDebugEnabled)
                    logger.Debug(ex);
            }
            finally
            {
                accessLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retrieve the PersistentList instance identified by the specified key.
        /// </summary>
        /// <param name="key">The key specifying the PersistentList instance to retrieve.</param>
        /// <returns>The PersistentList instance identified by the specified key.</returns>
        public PersistentList GetList(string key)
        {
            accessLock.EnterUpgradeableReadLock();
            try
            {
                PersistentList result = null;
                if (!lists.TryGetValue(key, out result))
                {
                    accessLock.EnterWriteLock();
                    try
                    {
                        result = new PersistentList();
                        lists.Add(key, result);
                    }
                    finally
                    {
                        accessLock.ExitWriteLock();
                    }
                }
                return result;
            }
            finally
            {
                accessLock.ExitUpgradeableReadLock();
            }
        }      
    }
}

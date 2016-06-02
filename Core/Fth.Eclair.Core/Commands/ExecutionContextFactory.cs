//----------------------------------------------------------------------------- 
// <copyright file=ExecutionContextFactory">
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
using System.Threading;

namespace Eclair.Commands
{
    /// <summary>
    /// A factory used to build execution context objects, which govern the behaviour of ECLAIR commands at runtime.
    /// </summary>
    public class ExecutionContextFactory
    {

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ExecutionContextFactory));

        private static readonly object singletonLock = new object();

        private static volatile ExecutionContextFactory instance;
       
        /// <summary>
        /// The singleton instance for the factory.
        /// </summary>
        public static ExecutionContextFactory Instance
        {
            get
            {
                if (instance == null)
                    lock (singletonLock)
                        if (instance == null)
                            instance = new ExecutionContextFactory();

                return instance;
            }
        }


        private  int cancelRequest;
        private  int commandRunning;

      
    
        private object _syncRoot;
       
        /// <summary>
        /// An object that can be used to synchronize concurrent access to the factory's methods and properties.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        private ExecutionContextFactory()
        {
            this.cancelRequest = 0;
            this.commandRunning = 0;        
        }


        /// <summary>
        /// Represents a ECLAIR command execution context under which a command can be signaled to cancel its execution.
        /// </summary>
        public class CancelableExecutionContext : IDisposable
        {

            /// <summary>
            /// Creates a new instance of the CancelableExecutionContext class.
            /// </summary>
            /// <returns>The newly created CancelableExecutionContext instance.</returns>
            public static CancelableExecutionContext Create()
            {
                return new CancelableExecutionContext();
             
            }

            private CancelableExecutionContext()
            {
                ExecutionContextFactory.Instance.clearCancelRequest();
                ExecutionContextFactory.Instance.incCommandRunning();
            }

            private bool disposed;

            /// <summary>
            /// Disposes of the CancelableExecutionContext instance.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);               
            }

            /// <summary>
            /// Disposes of the CancelableExecutionContext instance.
            /// </summary>
            /// <param name="disposing">True if managed resources should be disposed.</param>
            protected void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        // Free managed resources.
                        if (ExecutionContextFactory.Instance.CancelPending)
                            logger.Warn("Command execution aborted!");

                        ExecutionContextFactory.Instance.decCommandRunning();
                        ExecutionContextFactory.Instance.clearCancelRequest();
                    }
                    // Free native resources.
                    // n/a
                    disposed = true;
                }
            }
        }

        /// <summary>
        /// Returns true if cancellation has been signaled.
        /// </summary>
        public bool CancelPending
        {
            get
            {
                lock (SyncRoot)
                {
                   return ((cancelRequest - commandRunning) >= 0);
                }
            }
        }

        private int incCommandRunning()
        {
            lock (SyncRoot)
            {
                this.commandRunning++;
                return this.commandRunning;
            }
        }

        private int decCommandRunning()
        {
            lock (SyncRoot)
            {
                this.commandRunning--;
                return this.commandRunning;
            }
        }

        private void clearCancelRequest()
        {
            lock (SyncRoot)
                this.cancelRequest = 0;
        }

        /// <summary>
        /// Sends a cancel signal to the execution context. 
        /// </summary>
        /// <returns></returns>
        public bool SignalCancel()
        {
            lock (SyncRoot)
            {
                cancelRequest++;
                logger.DebugFormat("Cancel signaled. Nb cancelRequest = {0}", cancelRequest);         
                return true;
            }
        }
    }
}

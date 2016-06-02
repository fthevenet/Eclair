//----------------------------------------------------------------------------- 
// <copyright file=PerfMonitor">
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
using System.Diagnostics;
using log4net;

namespace Eclair.Tools
{
    /// <summary>
    /// A class that encapsulate the results measured with a PerfMon object.
    /// </summary>
    public class PerfMonitorResult
    {
        /// <summary>
        /// Initializes a new instance of the PerfMonitorResult class. 
        /// </summary>
        public PerfMonitorResult()
            : this(string.Empty) { }

        /// <summary>
        /// Initializes a new instance of the PerfMonitorResult class with the specified label.
        /// </summary>
        /// <param name="label">The specified label</param>
        public PerfMonitorResult(string label)
        {
            this.Label = label;
        }

        /// <summary>
        /// Get or set the label.
        /// </summary>
        public string Label
        {
            get;
            set;
        }

        /// <summary>
        ///  Get or set the elapsed time.
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get;
            set;
        }

        /// <summary>
        /// Return the label and elapsed time as a string.
        /// </summary>
        /// <returns>A string representing the label and elapsed time.</returns>
        public override string ToString()
        {
            return this.Label +": "+ this.ElapsedTime.TotalMilliseconds.ToString() + " ms";
        }
    }

    /// <summary>
    /// A callback used to log the results of the monitor.
    /// </summary>
    /// <param name="message">The message added to log.</param>
    public delegate void PerfMonitorOutputCallback(string message);

    /// <summary>
    /// A utility class that measures and reports the execution time of a portion of code. 
    /// </summary>
    /// <remarks>
    /// A time interval is measured from the creation of the object to its disposal, enabling the use of a "using" statement 
    /// to measure the time taken to run the code contain within the brackets of the statement.
    /// </remarks> 
    public class PerfMonitor : IDisposable
    {
        static private ILog logger = LogManager.GetLogger(typeof(PerfMonitor));
        private string message;
        private PerfMonitorResult elapsed;
        private Stopwatch sw;
        private OutputModeEnum outputMode;
        private PerfMonitorOutputCallback writeCallback;
        private bool enabled;

        private enum OutputModeEnum { Return, Internal, Callback }

        /// <summary>
        /// Returns a new instance of the PerfMonitor class.
        /// </summary>
        /// <param name="message">The message associated to the perf monitor.</param>
        /// <param name="writeCallback">The callback that will be invoked to log the results of the monitor.</param>
        /// <returns>The newly created PerfMonitor instance.</returns>
        public static PerfMonitor Create(string message, PerfMonitorOutputCallback writeCallback) { return new PerfMonitor(null, message, OutputModeEnum.Callback , writeCallback); }

        /// <summary>
        /// Returns a new instance of the PerfMonitor class.
        /// </summary>
        /// <param name="message">The message associated to the perf monitor.</param>
        /// <returns>The newly created PerfMonitor instance.</returns>
        public static PerfMonitor Create(string message) { return new PerfMonitor(null, message,OutputModeEnum.Internal, null); }

        /// <summary>
        /// Returns a new instance of the PerfMonitor class.
        /// </summary>
        /// <param name="elapsed">An PerfMonitorResult object that will be used to store the results of the monitor.</param>
        /// <returns>The newly created PerfMonitor instance.</returns>
        public static PerfMonitor Create(PerfMonitorResult elapsed) { return new PerfMonitor(elapsed, string.Empty, OutputModeEnum.Return,null); }    

        private PerfMonitor(PerfMonitorResult elapsed, string message, OutputModeEnum logMode, PerfMonitorOutputCallback writeCallback)
        {
            this.enabled = (logMode != OutputModeEnum.Internal) || logger.IsDebugEnabled;
            if (enabled)
            {
                this.elapsed = elapsed;
                this.outputMode = logMode;
                this.writeCallback = writeCallback;
                this.message = message;
                this.sw = new Stopwatch();
                this.sw.Start();
            }
        }

        #region IDisposable Members

        private bool disposed;

        /// <summary>
        /// Dispose the perf monitor.
        /// </summary>
        /// <remarks>
        /// Disposing the perf monitor will stop the timer and store the result to a PerfMonitorResult object or invoke the logging callback.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Dispose the perf monitor.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>     
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (enabled && this.sw != null)
                {
                    this.sw.Stop();
                    switch (outputMode)
                    {
                        case OutputModeEnum.Return:
                            this.elapsed.ElapsedTime = this.sw.Elapsed;
                            break;

                        case OutputModeEnum.Internal:
                            logger.Debug(this.message + ": " + this.sw.ElapsedMilliseconds.ToString() + " ms");
                            break;

                        case OutputModeEnum.Callback:
                            if (writeCallback != null) writeCallback(this.message + ": " + this.sw.ElapsedMilliseconds.ToString() + " ms");
                            break;
                    }
                }
            }
            disposed = true;
        }

        #endregion
    }
}


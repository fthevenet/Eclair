//----------------------------------------------------------------------------- 
// <copyright file=Log4netConfigTools">
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
using log4net;
using log4net.Core;
using log4net.Appender;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace Eclair.Tools
{

    /// <summary>
    /// Provides utility methods to manage and configure log4net loggers and appenders.
    /// </summary>
    public static class Log4netConfigTools
    {

        /// <summary>
        /// Set the log level to the root element.
        /// </summary>
        /// <param name="level">The desired level.</param>
        public static void SetRootLevel(Level level)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.Level = level;
            hierarchy.RaiseConfigurationChanged(EventArgs.Empty);                   
        }

        /// <summary>
        /// Adds an appender to all loggers.
        /// </summary>
        /// <param name="appender">The appender to add.</param>
        public static void AddAppenderToRoot(log4net.Appender.IAppender appender)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.AddAppender(appender);
        }

        /// <summary>
        /// Adds an file appender to all loggers.
        /// </summary>
        /// <param name="name">The name of the appender.</param>
        public static void AddFileAppenderToRoot(string name)
        {
            log4net.Appender.FileAppender appender = new log4net.Appender.FileAppender();
            appender.Name = name;
            appender.LockingModel = new FileAppender.MinimalLock();
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.AddAppender(appender);
        }

        /// <summary>
        /// Sets the level for a named logger
        /// </summary>
        /// <param name="loggerName">The logger to modify.</param>
        /// <param name="levelName">The level to set the logger to.</param> 
        public static void SetLevel(string loggerName, Level levelName)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(loggerName);
            log4net.Repository.Hierarchy.Logger l = (log4net.Repository.Hierarchy.Logger)log.Logger;

            l.Level = levelName;
        }
        
        /// <summary>
        /// Adds an appender to a logger
        /// </summary>
        /// <param name="loggerName">The logger to modify.</param>
        /// <param name="appender">The appender to add.</param>
        public static void AddAppender(string loggerName, log4net.Appender.IAppender appender)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(loggerName);
            log4net.Repository.Hierarchy.Logger l = (log4net.Repository.Hierarchy.Logger)log.Logger;           
            l.AddAppender(appender);
        }

        /// <summary>
        /// Finds a named appender already attached to a logger
        /// </summary>
        /// <param name="appenderName">The name of the appender to find.</param>
        /// <returns>The attached appender.</returns>
        public static log4net.Appender.IAppender FindAppender(string appenderName)
        {
            foreach (log4net.Appender.IAppender appender in log4net.LogManager.GetRepository().GetAppenders())
            {
                if (appender.Name == appenderName)
                    return appender;
            }

            throw new ArgumentOutOfRangeException("appenderName", string.Format("Failed to find an appender named \"{0}\"", appenderName));
        }

      
        /// <summary>
        /// Configures the specified file appender attached to the root, or adds one if it doesn't exist.
        /// </summary>
        /// <param name="name">The name of the appender to configure</param>
        /// <param name="fileName">The file name value for the appender.</param>
        /// <param name="threshold">The threshold for the appender.</param>
        public static void SetupRootFileAppender(string name, string fileName, Level threshold)
        {
            IAppender appender = Log4netConfigTools.FindAppender(name);
            if (appender == null)
                Log4netConfigTools.AddFileAppenderToRoot(name);

            Log4netConfigTools.SetupFileAppender(name, fileName, threshold);
        }

        /// <summary>
        /// Configures the specified file appender.
        /// </summary>
        /// <param name="name">The name of the appender to configure</param>
        /// <param name="fileName">The file name value for the appender.</param>
        /// <param name="threshold">The threshold for the appender.</param>
        public static void SetupFileAppender( string name, string fileName, Level threshold)
        {
            FileAppender appender = (FileAppender)FindAppender(name);
            appender.File = fileName;
            appender.AppendToFile = true;
            appender.Threshold = threshold;

            log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout();
            layout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            layout.ActivateOptions();

            appender.Layout = layout;
            appender.ActivateOptions();

        }

    }
}

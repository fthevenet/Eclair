//----------------------------------------------------------------------------- 
// <copyright file=CommandFactory">
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
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Eclair.Tools;
using Eclair.Commands;
using Eclair.Exceptions;
using log4net;

namespace Eclair.Commands
{
    /// <summary>
    /// Provides methods for creating ICommand objects from keywords passed as string. 
    /// Also provides methods to load external command libraries. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the class responsible for registering individual command classes from external assemblies,
    /// known as "command libraries", and instantiate new objects for the <see cref="CommandProcessor"/> instance to execute.
    /// </para>
    /// <para>
    /// The command factory uses reflection to browse through all types in specified assemblies and registers
    /// the eligible types as commands, alongside its properties such as its associated keyword, category and inline help data.
    /// </para>
    /// <para>
    /// To be registered as a command by the factory, a type must be an instantiable class that implements
    /// the <see cref="ICommand"/> interface. It also need to be decorated with a <see cref="CommandInfoAttribute"/> attribute which
    /// specifies the command’s keyword and other properties.
    /// </para>
    /// <para>
    /// The <see cref="CommandFactory"/> class also provides methods to return a new instance of a command when given its keyword,
    /// as well as methods to returns lists of all registered commands, categories, etc…
    /// </para> 
    /// <para>
    /// It is not possible to explicitly create an instance of this class; its methods and properties are only 
    /// accessible through the static member CommandFactory.Instance.
    /// </para>
    /// </remarks>
    public sealed class CommandFactory
    {
        #region Logger initialization

        static private readonly ILog logger = LogManager.GetLogger(typeof(CommandFactory));

        #endregion

        #region Singleton management

        static private readonly object singletonLock = new object();
        static private volatile CommandFactory instance;

        /// <summary>
        /// Gets a singleton instance for the CommandFactory class.
        /// </summary>
        static public CommandFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (singletonLock)
                    {
                        if (instance == null)
                            instance = new CommandFactory();
                    }
                }
                return CommandFactory.instance;
            }
        }

        #endregion

        #region Ctors

        private ReaderWriterLockSlim accessLock;

        private CommandFactory()
        {
            accessLock = new ReaderWriterLockSlim();
            this.registeredLibraries = new List<AssemblyInfoRetriever>();
            availableCmds = new Dictionary<string, Dictionary<string, CommandInfoAttribute>>(new CiEqualityComparer());

            // Add root category
            availableCmds.Add("", new Dictionary<string, CommandInfoAttribute>(new CiEqualityComparer()));

            // Register base commands
            using (PerfMonitor.Create("Registering built-in commands", s => logger.Debug(s)))
                this.registerCommands(Assembly.GetExecutingAssembly());

            //load libraries in libFolder
            using (PerfMonitor.Create("Registering commands from external libraries"))
            {
                foreach (string s in Directory.GetFiles(this.LibFolder, "Eclair.*.dll"))
                {
                    try
                    {
                        this.registerCommands(s);
                    }
                    catch (Exception ex)
                    {
                        CommandProcessor.TerminalOutput.ErrorFormat("Failed to register command in assembly {0}: {1}", s, ex.Message);
                        logger.Error("Error registering command", ex);
                    }
                }
            }
        }
        #endregion

        #region Register Command Info from library

        private int registerCommands(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException(string.Format("File {0} doesn't exist", path), "path");

            using (PerfMonitor.Create("Registering commands in assembly " + Path.GetFileName(path), s => logger.Debug(s)))
                return registerCommands(Assembly.LoadFrom(path));
        }

        private int registerCommands(Assembly myAsm)
        {

            if (logger.IsDebugEnabled)
                logger.DebugFormat("Registering commands in assembly {0}", myAsm.FullName);

            int nbCmd = 0;
            int clntFactory = 0;
            foreach (Type t in myAsm.GetTypes())
            {
                if (t.GetInterface(typeof(IClientProxyFactory).FullName) != null && (t != typeof(NullClientProxyFactory)))
                {
                    if (this.clientProxyFactory != null)
                    {
                        string warnMsg = String.Format(
                            "An instance of \"{0} => {1}\" is already registered as the clientProxyFactory and will be overwritten by an instance of \"{2} => {3}\"",
                            this.clientProxyFactory.GetType().Assembly.FullName,
                            this.clientProxyFactory.GetType().FullName,
                            myAsm.FullName,
                            t.FullName);

                        CommandProcessor.TerminalOutput.Warn(warnMsg);
                        logger.WarnFormat(warnMsg);
                    }
                    this.clientProxyFactory = (IClientProxyFactory)myAsm.CreateInstance(t.FullName);

                    clntFactory++;

                    if (logger.IsDebugEnabled)
                        logger.DebugFormat("clientProxyFactory {0} successfully registered", t.FullName);
                }

                if (t.GetInterface(typeof(ICommand).FullName) != null)
                {
                    foreach (CommandInfoAttribute ci in t.GetCustomAttributes(typeof(CommandInfoAttribute), true))
                    {
                        ci.CommandType = t;
                        if (!availableCmds.ContainsKey(ci.Category))
                            availableCmds.Add(ci.Category, new Dictionary<string, CommandInfoAttribute>(new CiEqualityComparer()));

                        if (availableCmds[ci.Category].ContainsKey(ci.Keyword))
                        {
                            string warnMsg = String.Format("A command with keyword \"{0}\" is already registered for category \"{1}\"", ci.Keyword, ci.Category);
                            CommandProcessor.TerminalOutput.Warn(warnMsg);
                            logger.WarnFormat(warnMsg);
                        }
                        else
                        {
                            availableCmds[ci.Category].Add(ci.Keyword, ci);
                            nbCmd++;

                            if (logger.IsDebugEnabled)
                                logger.DebugFormat("Command {0} successfully registered", t.FullName);
                        }
                    }
                }
            }

            if (nbCmd + clntFactory > 0)
                this.registeredLibraries.Add(new AssemblyInfoRetriever(myAsm));

            return nbCmd;

        }

        #endregion

        #region Properties & indexers

        private string libFolder;
        /// <summary>
        /// Get the path to the default command library folder.
        /// </summary>
        public string LibFolder
        {
            get
            {
                accessLock.EnterUpgradeableReadLock();
                try
                {
                    if (string.IsNullOrEmpty(this.libFolder))
                    {
                         accessLock.EnterWriteLock();
                         try
                         {
                             this.libFolder = String.Format("{0}\\CommandLibraries\\", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                             if (!Directory.Exists(this.libFolder))
                                 Directory.CreateDirectory(this.libFolder);
                         }
                         finally
                         {
                             accessLock.ExitWriteLock();
                         }
                    }
                    return this.libFolder;
                }
                finally
                {
                    accessLock.ExitUpgradeableReadLock();
                }
            }
        }

        private volatile IClientProxyFactory clientProxyFactory = null;

        /// <summary>
        /// Get the registered Client Proxy Factory.
        /// </summary>
        /// <exception cref="ClientProxyFactoryNullException">
        /// Throws a ClientProxyFactoryNullException if no ClientProxyFactory is currently registered.
        /// </exception>
        public IClientProxyFactory ClientProxyFactory
        {
            get
            {
                accessLock.EnterUpgradeableReadLock();
                try
                {
                    if (clientProxyFactory == null)
                    {
                        accessLock.EnterWriteLock();
                        try
                        {
                            if (clientProxyFactory == null)
                                clientProxyFactory = new NullClientProxyFactory();                                                                          
                        }
                        finally
                        {
                            accessLock.ExitWriteLock();
                        }
                    }
                    return this.clientProxyFactory;
                }
                finally
                {
                    accessLock.ExitUpgradeableReadLock();
                }
            }
        }

        private Dictionary<string, Dictionary<string, CommandInfoAttribute>> availableCmds;

        private List<AssemblyInfoRetriever> registeredLibraries;
        /// <summary>
        /// Gets a list of registered command libraries.
        /// </summary>
        public IEnumerable<AssemblyInfoRetriever> RegisteredLibraries
        {
            get
            {
                accessLock.EnterReadLock();
                try
                {
                    return this.registeredLibraries;
                }
                finally
                {
                    accessLock.ExitReadLock();
                }
            }          
        }
        
        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether the specified category exists.
        /// </summary>
        /// <param name="category">The name of the category.</param>
        /// <returns>
        /// True if at least one of the registered command refers to the specified category; false otherwise.
        /// </returns>
        public bool CategoryExists(string category)
        {
            return CategoryExists(category, false);
        }

        /// <summary>
        /// Determines whether the specified category exists.
        /// </summary>
        /// <param name="category">The name of the category.</param>
        /// <param name="searchAll">True to also search for categories that don't contain any browsable category, false, otherwise.</param>
        /// <returns>
        /// True if at least one of the registered command refers to the specified category; false otherwise.
        /// </returns>
        public bool CategoryExists(string category, bool searchAll)
        {           
            accessLock.EnterReadLock();
            try
            {
                return categoryExists(category, searchAll);
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Determines whether the specified command exists for the specified category.
        /// </summary>
        /// <param name="category">The name of the category.</param>
        /// <param name="commandKeyword">The keyword for the command to check.</param>
        /// <returns>
        /// True is the command passed as a parameters exists in the provided category; false otherwise.
        /// </returns>
        public bool CommandExists(string category, string commandKeyword)
        {
            accessLock.EnterReadLock();
            try
            {
                return commandExists(category, commandKeyword);

            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Retrieves a list of all categories that contain a command with the specified keyword.
        /// </summary>
        /// <param name="commandKeyword">Keyword of the command.</param>
        /// <returns>
        /// A List of all categories that contain a command with the specified keyword.
        /// </returns>
        public IEnumerable<string> FindCategories(string commandKeyword)
        {
            accessLock.EnterReadLock();
            try
            {
                return findCategories(commandKeyword);
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Retrieves a CommandInfoAttribute object for the specified command keyword and category. 
        /// A return value indicates whether a CommandInfoAttribute could be identified for the specified keyword and category.
        /// </summary>
        /// <param name="category">Name of the category.</param>
        /// <param name="commandKeyword">Keyword of the command.</param>
        /// <param name="info">The resulting CommandInfoAttribute instance.</param>
        /// <returns>True if the method successfully retrieved a CommandInfoAttribute for the specified keyword.</returns>
        public bool TryGetCommandInfo(string category, string commandKeyword, out CommandInfoAttribute info)
        {
            accessLock.EnterReadLock();
            try
            {
                return tryGetCommandInfo(category, commandKeyword, out info);
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Loads a command library from the specified path.
        /// </summary>
        /// <param name="path">The path to load the command library from.</param>
        /// <returns>The number of command successfully registered from the library.</returns>
        public int LoadCommandLibrary(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("The  path provided to load command library from is null or empty");
        
            accessLock.EnterWriteLock();
            try
            {
                return registerCommands(path);
            }
            finally
            {
                accessLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Loads a command library from the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to load the command library from.</param>
        /// <returns>The number of command successfully registered from the library.</returns>
        public int LoadCommandLibrary(Assembly assembly)
        {

            accessLock.EnterWriteLock();
            try
            {
                return registerCommands(assembly);
            }
            finally
            {
                accessLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Create and returns a new ICommand instance from the specified argument list.
        /// </summary>
        /// <param name="category">The current command category.</param>
        /// <param name="args">The specified argument list.</param>
        /// <returns>The instance of the class representing the command specified in the argument list.</returns>
        /// <exception cref="Eclair.Exceptions.UnknownCommandException">An UnknownCommandException is thrown if no valid ICommand can be instantiated from the provided argument list.</exception>
        public ICommand CreateCommand(string category, List<string> args)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            if (args == null)
                throw new ArgumentNullException("args");

            CommandInfoAttribute ci = null;
            foreach (string s in args)
            {
                if (TryGetCommandInfo(category, s, out ci))
                {
                    args.Remove(s);
                    return (ICommand)ci.CommandType.Assembly.CreateInstance(ci.CommandType.FullName);
                }
            }
            throw new UnknownCommandException();
        }


        /// <summary>
        /// Returns an enumeration of all browsable commands registered by the factory for a given category.
        /// </summary>
        /// <param name="category">The category to retrieve browsable commands from.</param>
        /// <returns>An enumeration of all browsable commands.</returns>
        public IEnumerable<CommandInfoAttribute> GetCommands(string category)
        {
            return this.GetCommands(category, false);
        }

        /// <summary>
        /// Returns an enumeration of all browsable commands registered by the factory for a given category.
        /// </summary>
        /// <param name="category">The category to retrieve browsable commands from.</param>
        /// <param name="includeHidden">Set to true to return all commands in the category, including the commands marked as "Not browsable".
        /// If false only the browsable commands are returned.</param>
        /// <returns>An enumeration of all browsable commands.</returns>
        public IEnumerable<CommandInfoAttribute> GetCommands(string category, bool includeHidden)
        {
            accessLock.EnterReadLock();
            try
            {
                return getCommands(category, includeHidden);
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets a list of registered command categories.
        /// </summary>
        /// <remarks>
        /// Categories with no browsable commands are ignored.
        /// </remarks>
        /// <returns>A list of registered command categories</returns>
        public IEnumerable<string> GetCategories()
        {
            return this.GetCategories(false);
        }

        /// <summary>
        ///  Gets a list of registered command categories.
        /// </summary>
        /// <param name="returnAll">True to retrieve all categories, including those containing no browsable commands, false otherwise.</param>
        /// <returns>A list of registered command categories</returns>
        public IEnumerable<string> GetCategories(bool returnAll)
        {
            accessLock.EnterReadLock();
            try
            {
                return getCategories(returnAll);
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        #endregion

        #region Private methods

        private  IEnumerable<string> getCategories(bool returnAll)
        {
            if (returnAll)
                return this.availableCmds.Keys.ToList<string>();

            return from k in this.availableCmds.Keys
                   where k.Length == 0 || getCommands(k, returnAll).Count() > 0
                   select k;
        }

        private IEnumerable<CommandInfoAttribute> getCommands(string category, bool includeHidden)
        {
            if (category == null)
                throw new ArgumentNullException("category");


            if (!this.availableCmds.ContainsKey(category))
                throw new ArgumentException("Unknown command category: " + category, "category");

            if (includeHidden)
                return this.availableCmds[category].Values.Where(c => !c.CommandType.IsSubclassOf(typeof(IClientProxyFactory)));

            return from c in this.availableCmds[category].Values
                   where !c.NotBrowsable && !c.CommandType.IsSubclassOf(typeof(IClientProxyFactory))
                   select c;
        }

        private bool commandExists(string category, string commandKeyword)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            if (commandKeyword == null)
                throw new ArgumentNullException("commandKeyword");


            if (this.availableCmds[category].ContainsKey(commandKeyword))
                return true;

            if (this.availableCmds["*"].ContainsKey(commandKeyword))
                return true;

            string[] ar = commandKeyword.Split('\\');
            if (ar.Length == 2 && this.availableCmds.ContainsKey(ar[0]))
                if (this.availableCmds[ar[0]].ContainsKey(ar[1]))
                    return true;

            return false;
        }

        private bool tryGetCommandInfo(string category, string commandKeyword, out CommandInfoAttribute info)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            if (commandKeyword == null)
                throw new ArgumentNullException("commandKeyword");

            info = null;

            if (this.availableCmds[category].TryGetValue(commandKeyword, out info))
                return true;

            if (this.availableCmds["*"].TryGetValue(commandKeyword, out info))
                return true;

            string[] ar = commandKeyword.Split('\\');
            if (ar.Length == 2 && this.availableCmds.ContainsKey(ar[0]))
                if (this.availableCmds[ar[0]].TryGetValue(ar[1], out info))
                    return true;

            return false;

        }

        private IEnumerable<string> findCategories(string commandKeyword)
        {

            if (commandKeyword == null)
                throw new ArgumentNullException("commandKeyword");


            List<string> categories = new List<string>();
            foreach (var cat in availableCmds)
            {
                if (cat.Value.ContainsKey(commandKeyword))
                    categories.Add(cat.Key);
            }

            return categories;
        }

        private bool categoryExists(string category, bool searchAll)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            if (searchAll)
                return this.availableCmds.ContainsKey(category);

            foreach (var k in availableCmds.Keys)
            {
                if (k.Equals(category, StringComparison.OrdinalIgnoreCase) && (k.Length == 0 || getCommands(k, false).Count() > 0))
                    return true;
            }
            return false;
        }

        #endregion
    }
}

//----------------------------------------------------------------------------- 
// <copyright file=CommandArgsExtension">
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
using System.Text.RegularExpressions;
using System.IO;

namespace Eclair.Tools
{

    /// <summary>
    /// Defines extension methods and utilities to manipulate command line arguments. 
    /// </summary>
    public static class CommandArgsExtension
    {
        /// <summary>
        /// Returns true if at list one argument with the specified name exists in the list.
        /// </summary>
        /// <param name="cmdLines">The IEnumerable in which to search for an argument.</param>
        /// <param name="name">The name of the argument.</param>
        /// <returns>True if the list contains the argument, false otherwise.</returns>
        public static bool ArgumentExists(this IEnumerable<string> cmdLines, string name)
        {
            return cmdLines.Contains(name, new Tools.CiEqualityComparer());
        }

        /// <summary>
        /// Returns all arguments matching the provided expression.
        /// </summary>
        /// <typeparam name="T">The type of the returned argument.</typeparam>
        /// <param name="cmdLines">The IEnumerable from which to retrieve arguments.</param>
        /// <param name="name">The name of the argument.</param>
        /// <param name="format">A regular expression describing the format of the value to retrieve.</param>
        /// <returns>An IEnumerable of type T.</returns>
        public static IEnumerable<T> GetAllArguments<T>(this IEnumerable<string> cmdLines, string name, string format)
        {
            string expression = string.Format(@"(?i)(?<name>{0})=(?<val>{1})", name, format);

            return 
                from s in cmdLines
                let m = Regex.Match(s, expression)
                where m.Groups["val"].Success
                let r = TryParse<T>(m.Groups["val"].Value)
                where r.Key == true
                select r.Value;

        }

        /// <summary>
        /// Returns the first argument matching the provided expression.
        /// </summary>
        /// <typeparam name="T">The type of the returned argument.</typeparam>
        /// <param name="cmdLines">The IEnumerable from which to retrieve arguments.</param>
        /// <param name="name">The name of the argument.</param>
        /// <param name="format">A regular expression describing the format of the value to retrieve.</param>
        /// <returns>Default(TSource) if not matching element is found, otherwise, the first matching element in cmdLine.</returns>
        public static T GetFirstArgument<T>(this IEnumerable<string> cmdLines, string name, string format)
        {
            return GetAllArguments<T>(cmdLines, name, format).FirstOrDefault();
        }

        /// <summary>
        /// Returns the first argument matching the provided expression.
        /// </summary>
        /// <typeparam name="T">The type of the returned argument.</typeparam>
        /// <param name="cmdLines">The IEnumerable from which to retrieve arguments.</param>
        /// <param name="name">The name of the argument.</param>
        /// <param name="format">A regular expression describing the format of the value to retrieve.</param>
        /// <param name="defaultValue">The default value to return in case not matching element can be found.</param>
        /// <returns>defaultValue if not matching element is found, otherwise, the first matching element in cmdLine.</returns>
        public static T GetFirstArgument<T>(this IEnumerable<string> cmdLines, string name, string format, T defaultValue)
        {
            var res = GetAllArguments<T>(cmdLines, name, format);
            if (res.Count() == 0)
                return defaultValue;
            else
                return res.First();
        }

        /// <summary>
        /// Returns the first argument that is formated as a rooted path.
        /// </summary>
        /// <remarks>
        /// This will returns both local and UNC formated paths.
        /// </remarks>
        /// <param name="cmdLines">The IEnumerable from which to retrieve arguments.</param>
        /// <returns>The first argument that is formated as a rooted path.</returns>
        public static string GetFirstPathFromArguments(this IEnumerable<string> cmdLines)
        {
            return cmdLines.GetAllPathFromArguments().FirstOrDefault();
        }

        /// <summary>
        /// Returns all the arguments that are formated as a rooted path.
        /// </summary>
        ///     /// <remarks>
        /// This will returns both local and UNC formated paths.
        /// </remarks>
        /// <param name="cmdLines">The IEnumerable from which to retrieve arguments.</param>
        /// <returns>An IEnumerable of all paths.</returns>
        public static IEnumerable<string> GetAllPathFromArguments(this IEnumerable<string> cmdLines)
        {
            return from p in cmdLines
                   where Path.IsPathRooted(p)
                   select p;
        }

        /// <summary>
        /// Attempts to parse the specified string to an instance of the type provided.
        /// </summary>
        /// <typeparam name="T">The type to parse the string to.</typeparam>
        /// <param name="value">The string to parse.</param>
        /// <returns>A key/value pair made out of a boolean that indicates whether the string successfully parsed to an instance of T or not,
        /// and the instance of T in case of success. If the parsing fails, default(T) is return.
        /// </returns>
        public static KeyValuePair<bool, T> TryParse<T>(string value)
        {

            if ( typeof(T).IsEnum)
            {
                return new KeyValuePair<bool, T>(true, (T)Enum.Parse(typeof(T), value, true));
            }

            if (typeof(T) == typeof(string))
                return new KeyValuePair<bool, T>(true, (T)(object)value);

            if (typeof(T) == typeof(int))
            {
                int i;
                bool res = int.TryParse(value, out i);
                return new KeyValuePair<bool, T>(res, (T)(object)i);
            }

            if (typeof(T) == typeof(long))
            {
                long l;
                bool res = long.TryParse(value, out l);
                return new KeyValuePair<bool, T>(res, (T)(object)l);
            }

            if (typeof(T) == typeof(DateTime))
            {
                DateTime d;
                bool res = DateTime.TryParse(value, out d);
                return new KeyValuePair<bool, T>(res, (T)(object)d);
            }

            if (typeof(T) == typeof(Double))
            {
                double d;
                bool res = double.TryParse(value, out d);
                return new KeyValuePair<bool, T>(res, (T)(object)d);
            }

            if (typeof(T) == typeof(Version))
            {
                Version v;
                bool res = Version.TryParse(value, out v);
                return new KeyValuePair<bool, T>(res, (T)(object)v);
            }

            if (typeof(T) == typeof(Type))
            {
                try
                {
                    var t = Type.GetType(value, true, true);
                    return new KeyValuePair<bool, T>(true, (T)(object)t);
                }
                catch
                {
                    return new KeyValuePair<bool, T>(false, default(T));
                }

            }

            return new KeyValuePair<bool, T>(false, default(T));
        }
    }
}

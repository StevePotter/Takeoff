using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace System
{
    public static class GoodieExtensions
    {
        public static T AddTo<T>(this T value, IList<T> addTo)
        {
            addTo.Add(value);
            return value;
        }

        public static IEnumerable<T> AddAllTo<T>(this IEnumerable<T> values, IList<T> addTo)
        {
            foreach (var v in values)
            {
                addTo.Add(v);
            }
            return values;
        }


        public static string PropertyName<T>(this T propertyContainer, Expression<Func<T, object>> getProperty)
        {
            var memberExpression = getProperty.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentNullException("getProperty", "Must be an expression returing a property value, like o => o.Foo");
            return memberExpression.Member.Name;
        }

        public static MemberInfo Member<T>(this T propertyContainer, Expression<Func<T, object>> getMember)
        {
            var memberExpression = getMember.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentNullException("getMember", "Must be an expression returing a property value, like o => o.Foo");
            return memberExpression.Member;
        }

        ///// <summary>
        ///// Shortcut for string.Join(string.Empty,values).  Instead you just write values.Join().  
        ///// </summary>
        ///// <param name="values"></param>
        ///// <returns></returns>
        //public static string Join(this IEnumerable<string> values)
        //{
        //    return values.Join(string.Empty);
        //}

        /// <summary>
        /// If the current value has characters and "otherstring" does as well, this joins them with the given separator.  If neither have characters, it returns string.Empty.  If one has characters, it returns that one without a separator.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="otherString"></param>
        /// <returns></returns>
        public static string JoinNonEmpty(this string value, string otherString, string separator)
        {
            var isValEmpty = string.IsNullOrEmpty(value);
            var isOtherEmpty = string.IsNullOrEmpty(otherString);
            if (isValEmpty && isOtherEmpty)
                return string.Empty;
            else if (!isValEmpty && !isOtherEmpty)
                return String.Concat(value, separator, otherString);
            else if (isValEmpty)
                return otherString;
            else
                return value;
        }

        /// <summary>
        /// If the value is null, it returns the default value supplied.  Otherwise it returns the original value cast as the default value's type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="val"></param>
        /// <param name="valIfNotNull"></param>
        /// <returns></returns>
        public static T ValueOr<T>(this object value, T defaultVal)
        {
            if (value == null)
                return defaultVal;
            else
                return (T)value;
        }

        /// <summary>
        /// If the value is null, it returns the default value for the type.  Otherwise it returns the original value cast as the default value's type.
        /// </summary>
        public static T ValueOr<T>(this object value)
        {
            return ValueOr(value, default(T));
        }

        /// <summary>
        /// Same as php's MD5 function
        /// //http://skysigal.xact-solutions.com/Blog/tabid/427/EntryId/425/Porting-the-Equivalent-of-PHP-rsquo-s-MD5-to-NET-platform-in-C.aspx
        /// //http://www.spiration.co.uk/post/1203/MD5%20in%20C%23%20-%20works%20like%20php%20md5%28%29%20example
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string MD5Hash(this string input)
        {
            using (var md5Provider = new MD5CryptoServiceProvider())
            {
                return string.Join(string.Empty, md5Provider.ComputeHash(Encoding.UTF8.GetBytes(input)).Select(b => b.ToString("x2")).ToArray());
            }
        }

        /// <summary>
        /// Useful when doing long string method chaining.  
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AppendWith(this string text, string append)
        {
            if (string.IsNullOrEmpty(text))
                return append;
            if (string.IsNullOrEmpty(append))
                return text;
            return string.Concat(text, append);
        }
        public static string PrependWith(this string text, string prepend)
        {
            if (string.IsNullOrEmpty(text))
                return prepend;
            if (string.IsNullOrEmpty(prepend))
                return text;
            return string.Concat(prepend, text);
        }

        /// <summary>
        /// Fluent format function.  Had to add "string" due to compiler conflict with teh static method.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatString(this string format, params object[] args)
        {
            return string.Format(format, args);
        }


        public static string EncodeBase64(this string data)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
        }

        public static string DecodeBase64(this string data)
        {
            var decoder = System.Text.Encoding.UTF8.GetDecoder();
            byte[] base64Bytes = Convert.FromBase64String(data);
            int charCount = decoder.GetCharCount(base64Bytes, 0, base64Bytes.Length);
            char[] decodedChars = new char[charCount];
            decoder.GetChars(base64Bytes, 0, base64Bytes.Length, decodedChars, 0);
            return new String(decodedChars);
        }

        public static string ToRelative(this DateTime value, RelativeDateTimeFormat format)
        {
            if (value.Kind != DateTimeKind.Utc)
                value = value.ToUniversalTime();
            var now = DateTime.UtcNow;
            var today = now.Date;
            var yesterday = today.Subtract(TimeSpan.FromDays(1));

            //happened within the past 24-48 hours
            var difference = now - value;
            if (difference.TotalMinutes < 1)
                return format.SecondsOld;
            if (difference.TotalMinutes == 1)
                return format.MinuteOld;
            if (difference.TotalHours < 1)
                return format.MinutesOldPre + ((int)difference.TotalMinutes).ToInvariant() + format.MinutesOldPost;
            if (difference.TotalHours < 2)
                return format.HourOld;
            if (value >= today)
                return format.TodayPre + value.ToString(DateTimeFormat.ShortTime);
            if (value >= yesterday)
                return format.YesterdayPre + value.ToString(DateTimeFormat.ShortTime);
            if ( format.OlderTimeEnabled )
                return value.ToString(format.OlderDatePattern) + format.OlderTimePre + value.ToString(DateTimeFormat.ShortTime);

            return value.ToString(format.OlderDatePattern);
        }


        /// <summary>
        /// If the element is null or its value is null, this returns the default value.  Otherwise it converts to the proper type.
        /// </summary>
        public static string ValueOrDefault(this XElement element)
        {
            if (element == null)
                return null;
            return element.Value;
        }

        /// <summary>
        /// If the element is null or its value is null, this returns the default value.  Otherwise it converts to the proper type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T ValueOrDefault<T>(this XElement element)
        {
            if (element == null || element.Value == null)
                return default(T);
            return element.Value.ConvertTo<T>();

        }

        /// <summary>
        /// Same as string.Empty but allows the method to be run on null instances, unlike string.Empty.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static bool EqualsCaseSensitive(this string value, string match)
        {
            if (value == null)
                return match == null;
            if (match == null)
                return false;
            return value.Equals(match, StringComparison.Ordinal);
        }

        public static string GetEmbeddedResourceString(this Type type, string resourceName)
        {
           // var names = type.Assembly.GetManifestResourceNames().ToArray();
                
            using (var stream = type.Assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static byte[] GetEmbeddedResourceBytes(this Type type, string resourceName)
        {            
            using (var stream = type.Assembly.GetManifestResourceStream(resourceName))
            {
                var buffer = new byte[stream.Length];
                using (var reader = new BinaryReader(stream))
                {
                    reader.Read(buffer, 0, buffer.Length);
                }
                return buffer;
            }

        }





        #region 2 way encryption

        /// <summary>
        /// Encrypts the string using the initialization vector ("IV") and encryption key supplied using TripleDES algorithm.  Can be decrypted.
        /// </summary>
        public static string EncryptTwoWay(this string inputValue, string initializationVector, string encryptionKey)
        {
            using (var provider = new TripleDESCryptoServiceProvider
                                                          {
                                                              IV = Convert.FromBase64String(initializationVector),
                                                              Key = Convert.FromBase64String(encryptionKey),
                                                          })
            {
                using (var mStream = new MemoryStream())
                {
                    // Create a CryptoStream using the MemoryStream 
                    // and the passed key and initialization vector (IV).
                    using (var cStream = new CryptoStream(mStream, provider.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] toEncrypt = new UTF8Encoding().GetBytes(inputValue);
                        // Write the byte array to the crypto stream and flush it.
                        cStream.Write(toEncrypt, 0, toEncrypt.Length);
                        cStream.FlushFinalBlock();
                        // Return the encrypted buffer.
                        return Convert.ToBase64String(mStream.ToArray());
                    }
                }
            }
        }


        /// <summary>
        /// Gets the decrypted value using TripleDES.
        /// </summary>
        public static string Decrypt(this string toDecrypt, string initializationVector, string encryptionKey)
        {
            using (var provider = new TripleDESCryptoServiceProvider
                                                          {
                                                              IV = Convert.FromBase64String(initializationVector),
                                                              Key = Convert.FromBase64String(encryptionKey),
                                                          })
            {
                byte[] inputEquivalent = Convert.FromBase64String(toDecrypt);
                using (var msDecrypt = new MemoryStream())
                {
                    // Create a CryptoStream using the MemoryStream and the passed key and initialization vector (IV).
                    using (var csDecrypt = new CryptoStream(msDecrypt,
                                                              provider.CreateDecryptor(),
                                                              CryptoStreamMode.Write))
                    {
                        csDecrypt.Write(inputEquivalent, 0, inputEquivalent.Length);
                        csDecrypt.FlushFinalBlock();
                        csDecrypt.Close();

                        //Convert the buffer into a string and return it.
                        return new UTF8Encoding().GetString(msDecrypt.ToArray());
                    }
                }
            }
        }

        #endregion


        #region Command Args

        /// <summary>
        /// Made for apps with a "main" method - console apps, windows services, winforms, etc.  Takes the lame string[] of parameters and turns them into a dictionary of key/value pairs.  
        /// </summary>
        /// <returns></returns>
        /// <example>
        /// 
        /// Command line: app.exe "input.txt" -a 2 -b -c "3 v"
        /// 
        /// args.ArgsToTable(new string[]{"in"}) will have these entries:
        /// "in": "input.txt"
        /// "a": "2"
        /// "b": null
        /// "c": "3 v"
        /// 
        /// </example>
        public static Dictionary<string, string> ArgsToTable(this string[] args, params string[] defaultArgs)
        {
            return ArgsToTable(args, true, defaultArgs);
        }

        /// <summary>
        /// Made for apps with a "main" method - console apps, windows services, winforms, etc.  Takes the lame string[] of parameters and turns them into a dictionary of key/value pairs.  
        /// </summary>
        public static Dictionary<string, string> ArgsToTable(this string[] args, bool makeLookupCaseSensitive, params string[] defaultArgs)
        {
            bool foundNamedArg = false;
            Dictionary<string, string> argTable = new Dictionary<string, string>(makeLookupCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < args.Length; i++)
            {
                var currArg = args[i];
                string value = null;
                if (currArg.StartsWith("-", StringComparison.Ordinal))
                {
                    if (!foundNamedArg)
                        foundNamedArg = true;
                    currArg = currArg.StartWithout("-", StringComparison.Ordinal);//cut off the -

                    //check for a cooresponding value
                    if (args.IsIndexInBounds(i + 1) && !args[i + 1].StartsWith("-"))
                    {
                        value = args[i + 1];
                        i++;
                    }
                }
                else
                {
                    if (foundNamedArg)
                        throw new ArgumentException("Argument '" + currArg + "' wasn't a named parameter.  Unnamed params must occur before any named ones.  Did you forget the dash before the argument name?");

                    if (!defaultArgs.IsIndexInBounds(i))
                        throw new ArgumentException("Arg #" + i.ToInvariant() + " didn't have an entry in defaultArgs");

                    value = currArg;
                    currArg = defaultArgs[i];
                }

                argTable.Add(currArg, value);
            }
            return argTable;
        }

        #endregion




        /// <summary>
        /// Takes this list of items and adds, removes, and updates items to match the target list provided.  This is used to synchronized two data sources.  Does not update the lists...that should be done in the various functions provided.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TToModify"></typeparam>
        /// <param name="sources">The source items that targets will be synchronized to.</param>
        /// <param name="toModify">The items that will be updated to match the source.</param>
        /// <param name="sourceKeySelector">Extracts a unique key to identify source items</param>
        /// <param name="targetKeySelector">Extracts a unique key to idtentify target items.</param>
        /// <param name="equalityComparer">Compares a source to target item.</param>
        /// <param name="addItems">Called when items need to be created in the target.  In this you would typically convert each item to the target type and add it to the database or something.</param>
        /// <param name="deleteItems">Called when items don't exist in the source object and need to be deleted.</param>
        /// <param name="updateItems">Called when two items with the same key aren't equal, according to equalityComparer.</param>
        public static void SyncTo<TToModify, TSource>(this IEnumerable<TToModify> toModify, IEnumerable<TSource> sources, Func<TSource, string> sourceKeySelector, Func<TToModify, string> toModifyKeySelector, Func<TSource, TToModify, bool> equalityComparer, Action<IEnumerable<TSource>> addItems, Action<IEnumerable<TToModify>> deleteItems, Action<IEnumerable<SourceTarget<TSource, TToModify>>> updateItems)
        {
            Dictionary<string, TSource> sourceDict = sources.ToDictionary(sourceKeySelector);
            Dictionary<string, TToModify> toModifyDict = toModify.ToDictionary(toModifyKeySelector);

            //first find any in recurly that don't exist in takeoff.  These will be deleted
            List<TToModify> toDelete = null;
            foreach (var toMod in toModifyDict.Values)
            {
                if (!sourceDict.ContainsKey(toModifyKeySelector(toMod)))
                {
                    if (toDelete == null)
                        toDelete = new List<TToModify>();
                    toDelete.Add(toMod);
                }
            }
            if (toDelete != null)
            {
                toDelete.Each(t => toModifyDict.Remove(toModifyKeySelector(t)));
            }

            //find any in source and not in toModify.  these will be added
            List<TSource> toAdd = null;
            foreach (var source in sourceDict.Values)
            {
                if (!toModifyDict.ContainsKey(sourceKeySelector(source)))
                {
                    if (toAdd == null)
                        toAdd = new List<TSource>();
                    toAdd.Add(source);
                }
            }

            
            //the only things left in toModifyplans are in both toModify and source.  now compare for equality
            List<SourceTarget<TSource, TToModify>> toUpdate = null;
            foreach (var toMod in toModifyDict.Values)
            {
                var source = sourceDict[toModifyKeySelector(toMod)];

                if (!equalityComparer(source, toMod))
                {
                    if (toUpdate == null)
                        toUpdate = new List<SourceTarget<TSource, TToModify>>();
                    toUpdate.Add(new SourceTarget<TSource, TToModify>
                    {
                        Source = source,
                        Target = toMod,
                    });
                }
            }


            if (toAdd != null)
                addItems(toAdd);
            if (toDelete != null)
                deleteItems(toDelete);
            if (toUpdate != null)
                updateItems(toUpdate);

        }
 
    }

    public class SourceTarget<TSource, TTarget>
    {
        public TSource Source { get; set; }
        public TTarget Target { get; set; }
    }

    public class RelativeDateTimeFormat
    {
        public string SecondsOld { get; set; }
        public string MinuteOld { get; set; }
        public string MinutesOldPre { get; set; }
        public string MinutesOldPost { get; set; }
        public string HourOld { get; set; }
        public string TodayPre { get; set; }
        public string YesterdayPre { get; set; }
        public DateTimeFormat OlderDatePattern { get; set; }
        public string OlderTimePre { get; set; }
        public bool OlderTimeEnabled { get; set; }


        public static readonly RelativeDateTimeFormat Field = new RelativeDateTimeFormat
        {
            SecondsOld = "Seconds ago",
            MinuteOld = "A minute ago",
            MinutesOldPre = "",
            MinutesOldPost = " minutes ago",
            HourOld = "1 hour ago",
            TodayPre = "Today at ",
            YesterdayPre = "Yesterday at ",
            OlderDatePattern = DateTimeFormat.ShortDate,
            OlderTimePre = " at ",
            OlderTimeEnabled = false
        };

    }
}

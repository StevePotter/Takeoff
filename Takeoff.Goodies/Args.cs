using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// Contains utility functions for dealing with arguments to functions, mostly checking whether an argument is valid.
    /// </summary>
    public static class Args
    {
        /// <summary>
        /// Throws an exception if the argument is null.
        /// </summary>
        public static void NotNull(object value)
        {
            NotNull(value, null);
        }
        /// <summary>
        /// Throws an exception if the argument is null.
        /// </summary>
        public static void NotNull(object value, string argName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argName);
            }
        
        }

        /// <summary>
        /// Throws an exception if the argument doesn't have at least one non-whitespace character.
        /// </summary>
        public static string HasChars(string value)
        {
            return HasChars(value, null);
        }

        /// <summary>
        /// Throws an exception if the argument doesn't have at least one non-whitespace character.
        /// </summary>
        public static string HasChars(string value, string argName)
        {
            return HasChars(value, argName, false);
        }

        /// <summary>
        /// Throws an exception if the argument doesn't have at least one character.
        /// </summary>
        public static string HasChars(string value, string argName, bool whitespaceCounts)
        {
            if (!value.HasChars(whitespaceCounts))
            {
                throw new ArgumentNullException(argName);
            }
            return value;
        }

        /// <summary>
        /// Throws an exception if the value passed is not a valid email address.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="argName"></param>
        /// <returns></returns>
        public static string Email(string value)
        {
            return Email(value, null);
        }
       
        /// <summary>
        /// Throws an exception if the value passed is not a valid email address.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="argName"></param>
        /// <returns></returns>
        public static string Email(string value, string argName)
        {
            if (!value.IsValidEmail())
            {
                throw new ArgumentException("Invalid email",argName);
            }
            return value;
        }

        /// <summary>
        /// Indicates whether the string passed is a valid email address or not.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            return ValidEmailRegex.IsMatch(email.ToLowerInvariant());//uppercase letters were screwing things up.  caps are okay so lowercase when checking
        }
        //regex taken from jquery validation plugin
        static Regex ValidEmailRegex = new Regex(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$");


        /// <summary>
        /// Throws an exception if the argument isn't a string to a file on the system.
        /// </summary>
        public static void FileExists(string path)
        {
            FileExists(path, null);
        }
        /// <summary>
        /// Throws an exception if the argument isn't a string to a file on the system.
        /// </summary>
        public static void FileExists(string path, string argName)
        {
            HasChars(path, argName);
            if (!File.Exists(path))
            {
                throw new ArgumentException("path does not exist", argName);
            }
        }

        /// <summary>
        /// Throws an exception if the argument doesn't have at least one character (not counting whitespace).  If it has characters, it lowercases the value using ToLowerInvariant.
        /// </summary>
        public static string HasCharsLower(string value)
        {
            return HasCharsLower(value, null);
        }

        /// <summary>
        /// Throws an exception if the argument doesn't have at least one character (not counting whitespace).  If it has characters, it lowercases the value using ToLowerInvariant.
        /// </summary>
        public static string HasCharsLower(string value, string argName)
        {
            if (!value.HasChars())
            {
                throw new ArgumentNullException(argName);
            }
            return value.ToLowerInvariant();
        }

        
        public static void HasItems(IEnumerable<ThreadStaticAttribute> value)
        {
            HasItems(value, null);
        }

        public static void HasItems(IEnumerable<ThreadStaticAttribute> value, string argName)
        {
            if (value == null || value.Count() <= 0)
            {
                throw new ArgumentException("Must have at least one item.", argName);
            }
        }

        public static void HasItems(Array value, string argName)
        {
            if (value == null || value.Length <= 0)
            {
                throw new ArgumentException("Must have at least one item.", argName);
            }            
        }

        /// <summary>
        /// Throws an exception if the value is not a valid data key value (0 or less).
        /// </summary>
        /// <param name="value"></param>
        public static void ValidKey(int value)
        {
            ValidKey(value, null);
        }

        /// <summary>
        /// Throws an exception if the value is not a valid data key value (0 or less).
        /// </summary>
        public static void ValidKey(int value, string argName)
        {
            if (!value.IsPositive())
            {
                throw new ArgumentException("invalid int key value", argName);
            }
        }

        public static void GreaterThanZero(int value, string argName)
        {
            Args.GreaterThan(value, 0, argName);
        }

        public static void GreaterThan(int value, int mustBeGreaterThan, string argName)
        {
            Compare(value, mustBeGreaterThan, ArgComparison.Greater, argName);
        }


        public static void GreaterThanZero(double value, string argName)
        {
            Args.GreaterThan(value, 0, argName);
        }

        public static void GreaterThan(double value, double mustBeGreaterThan, string argName)
        {
            Compare(value, mustBeGreaterThan, ArgComparison.Greater, argName);
        }

        public static void Compare(double value, double compareTo, ArgComparison comparison, string argName)
        {
            switch (comparison)
            {
                case ArgComparison.Greater:
                    if (value <= compareTo)
                        throw new ArgumentException("value " + value + " must be greater than " + compareTo, argName);
                    break;
                case ArgComparison.GreaterOrEqualTo:
                    if (value < compareTo)
                        throw new ArgumentException("value " + value + " must be greater than or equal to " + compareTo, argName);
                    break;
                case ArgComparison.Less:
                    if (value >= compareTo)
                        throw new ArgumentException("value " + value + " must be less than " + compareTo, argName);
                    break;
                case ArgComparison.LessOrEqualTo:
                    if (value > compareTo)
                        throw new ArgumentException("value " + value + " must be less than or equal to " + compareTo, argName);
                    break;
                case ArgComparison.Equal:
                    if (value != compareTo)
                        throw new ArgumentException("value " + value + " must be equal to " + compareTo, argName);
                    break;
                case ArgComparison.NotEqual:
                    if (value == compareTo)
                        throw new ArgumentException("value " + value + " must not be equal to " + compareTo, argName);
                    break;
            }
        }

        public static void Compare(int value, int compareTo, ArgComparison comparison, string argName)
        {
            switch (comparison)
            {
                case ArgComparison.Greater:
                    if (value <= compareTo)
                        throw new ArgumentException("value " + value + " must be greater than " + compareTo, argName);
                    break;
                case ArgComparison.GreaterOrEqualTo:
                    if (value < compareTo)
                        throw new ArgumentException("value " + value + " must be greater than or equal to " + compareTo, argName);
                    break;
                case ArgComparison.Less:
                    if (value >= compareTo)
                        throw new ArgumentException("value " + value + " must be less than " + compareTo, argName);
                    break;
                case ArgComparison.LessOrEqualTo:
                    if (value > compareTo)
                        throw new ArgumentException("value " + value + " must be less than or equal to " + compareTo, argName);
                    break;
                case ArgComparison.Equal:
                    if (value != compareTo)
                        throw new ArgumentException("value " + value + " must be equal to " + compareTo, argName);
                    break;
                case ArgComparison.NotEqual:
                    if (value == compareTo)
                        throw new ArgumentException("value " + value + " must not be equal to " + compareTo, argName);
                    break;
            }
        }    
    }

    public enum ArgComparison
    {
        Greater,
        GreaterOrEqualTo,
        Less,
        LessOrEqualTo,
        Equal,
        NotEqual,
    }
}

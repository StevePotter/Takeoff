using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Takeoff
{

    /// <summary>
    /// Exception thrown when a thing could not be found, or was found but the wrong type was indicated.
    /// </summary>
    public class ThingNotFoundException : Exception
    {
        public ThingNotFoundException(string msg)
            : base(msg)
        {
        }

        public ThingNotFoundException(string id, string itemType)
            : base(itemType + " with id '" + id + "' was not found")
        {
        }
    }

    public class NoPermissionException : Exception
    {
        public NoPermissionException(string thingType = null)
        {
        }
    }

    /// <summary>
    /// An exception that occurs when an unvalid type was passed.
    /// </summary>
    public partial class InvalidTypeException : System.Exception
    {
        public InvalidTypeException(Type typePassed, Type validType)
            : this(typePassed, validType, String.Empty)
        {
        }

        public InvalidTypeException(string message)
            : base(message)
        {
        }

        public InvalidTypeException(Type typePassed, Type validType, string message)
            : base(message)
        {
            TypePassed = typePassed;
            ValidType = validType;
        }

        /// <summary>
        /// The invalid type that was passed to the method that raised the exception.
        /// </summary>
        public Type TypePassed
        {
            get; private set; 
        }

        /// <summary>
        /// The valid type that was expected.
        /// </summary>
        public Type ValidType
        {
            get; private set; 
        }
    }

}

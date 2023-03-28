using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Takeoff
{
    public class InvalidEnumValueException : Exception
    {

        public InvalidEnumValueException(System.Enum value):this(value, "Invalid enum value: " + value.ToString())
        {
        }

        public InvalidEnumValueException(System.Enum value, string message): base(message)
        {
            Value = value;
        }

        public System.Enum Value { get; private set; }
    }
}

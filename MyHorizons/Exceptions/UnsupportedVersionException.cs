using System;

namespace MyHorizons.Exceptions
{
    public class UnsupportedVersionException : Exception
    {
        public UnsupportedVersionException()
        {
        }

        public UnsupportedVersionException(string message) : base(message)
        {
        }

        public UnsupportedVersionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

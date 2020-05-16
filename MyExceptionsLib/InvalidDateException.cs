using System;

namespace MyExceptionsLib
{
    public class InvalidDateException : Exception
    {
        public InvalidDateException(string msg) : base(msg)
        {
        }
    }
}

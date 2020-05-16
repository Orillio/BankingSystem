using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyExceptionsLib
{
    public class DepositException : Exception
    {
        public DepositException(string msg) : base(msg)
        {
        }
    }
}

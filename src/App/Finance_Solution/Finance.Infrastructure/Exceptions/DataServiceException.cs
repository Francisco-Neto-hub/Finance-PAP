using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Infrastructure.Exceptions
{
    public class DataServiceException : Exception
    {
        public DataServiceException(string message, Exception inner) : base(message, inner) { }
    }
}

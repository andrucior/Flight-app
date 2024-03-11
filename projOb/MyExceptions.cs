using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    public class InvalidNumberOfArgsException: Exception
    {
        public InvalidNumberOfArgsException() : base() { }
        public InvalidNumberOfArgsException(string message) : base(message) { }
        public InvalidNumberOfArgsException(string message, Exception exception): base(message, exception) { }
    }
}

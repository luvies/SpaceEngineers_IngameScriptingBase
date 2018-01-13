using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build
{
    class CustomBlockException : Exception
    {
        public CustomBlockException() { }
        public CustomBlockException(string message) : base(message) { }
        public CustomBlockException(string message, Exception inner) : base(message, inner) { }
    }
}

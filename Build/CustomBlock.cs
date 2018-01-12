using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build
{
    class CustomBlock
    {
        public string BlockOpen { get; }
        public string BlockClose { get; }
        Func<string, string> _processFunc;

        public CustomBlock(string open, string close, Func<string, string> processFunc)
        {
            BlockOpen = open;
            BlockClose = close;
            _processFunc = processFunc;
        }

        public bool ShouldOpen(string test) => test == BlockOpen;
        public bool ShouldClose(string test) => test == BlockClose;

        public void ProcessBlock(ref StringBuilder builder, ref StringBuilder buffer) => builder.Append(_processFunc(buffer.ToString()));
    }
}

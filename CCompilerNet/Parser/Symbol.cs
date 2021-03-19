using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CCompilerNet.Parser
{
    public class Symbol
    {
        public string Type { get; set; }
        public Kind Kind { get; set; }
        public int Index { get; set; }
        public LocalBuilder LocalBuilder { get; set; }

        public Symbol(string type, Kind kind)
        {
            Type = type;
            Kind = kind;
            Index = 0;
        }
    }
}

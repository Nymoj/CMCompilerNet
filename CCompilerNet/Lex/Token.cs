using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCompilerNet.Lex
{
    public enum TokenType
    {
        Keyword,
        ID,
        Const,
        StringLiteral,
        SpecialSymbol,
        Operator,
    }

    public class Token
    {
        public TokenType _type { get; set; }
        public string _value { get; set; }

        public Token(TokenType type, string value)
        {
            _type = type;
            _value = value;
        }

        public override string ToString()
        {
            return String.Format("<{0}>{1}</{2}>", _type, _value, _type);
        }
    }
}

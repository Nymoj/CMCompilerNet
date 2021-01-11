using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CCompilerNet.Lex
{
    public class Lexer
    {
        private StreamReader _stream;
        private string _line;
        private bool _commentMode;
        private int _pos;
        
        public Lexer(string path)
        {
            _stream = new StreamReader(path);

            // initializing the first line
            _line = _stream.ReadLine();
            _pos = 0;
            _commentMode = false;
        }

        public Token GetNextToken()
        {
            return null;
        }

        public Token Peek()
        {
            return null;
        }

        private bool IsEndLine()
        {
            return _pos >= _line.Length;
        }

        private bool IsMatch(char symbol)
        {
            return _line[_pos] == symbol;
        }

        private void SkipWhiteSpace()
        {
            while (IsMatch(' ') || IsMatch('\t'))
            {
                _pos++;
            }
        }
    }
}

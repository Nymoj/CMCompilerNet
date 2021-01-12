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
            Token token = null;
            int startPos = _pos;

            if (IsConstant())
            {
                token = new Token(TokenType.Const, CutTokenFromLine(_pos));
            }

            return token;
        }

        public Token Peek()
        {
            return null;
        }

        private string CutTokenFromLine(int start)
        {
            return _line.Substring(start, _pos - start);
        }

        private bool IsConstant()
        {
            return IsNumConst() || IsCharConst();
        }

        private bool IsNumConst()
        {
            // DIGIT++

            int originalPos = _pos;

            if (!char.IsDigit(_line[_pos]))
            {
                return false;
            }

            while (char.IsDigit(_line[_pos]))
            {
                _pos++;
            }

            if (!IsMatch('.'))
            {
                return true;
            }

            _pos++;

            if (!char.IsDigit(_line[_pos]))
            {
                _pos = originalPos;
                return false;
            }

            while (char.IsDigit(_line[_pos]))
            {
                _pos++;
            }

            return true;
        }

        private bool IsCharConst()
        {
            // LETTER+
            int originalPos = _pos;

            if (!IsMatch('\''))
            {
                return false;
            }

            _pos++;

            if (!IsLetterOrDigit(_line[_pos]))
            {
                _pos = originalPos;
                return false;
            }

            _pos++;

            if (!IsMatch('\''))
            {
                _pos = originalPos;
                return false;
            }

            _pos++;

            return true;
        }

        private bool IsLetterOrDigit(char symbol)
        {
            return char.IsDigit(symbol) || char.IsLetter(symbol);
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

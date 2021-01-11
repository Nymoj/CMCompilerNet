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

        private bool IsKeyword()
        {
            // LETTER+

            // language keywords

            List<string> keywords = new List<string>{
                    "auto", "break", "case", "char",
		    "const", "continue", "default", "do",
		    "double", "else", "enum", "extern",
		    "float", "for", "goto", "if",
		    "int", "long", "register", "return",
		    "short", "signed", "sizeof", "static",
		    "struct", "switch", "typedef", "union",
		    "unsigned", "void", "volatile", "while"

            };

            string result = "";
            int originalPos = _pos;

            if ( !char.IsLetter( _line[_pos] ) )
            {
                return false;
            }

            while ( char.IsLetter( _line[_pos] ) )
            {
                result += _line[_pos++];
            }

            // check if the word is a keyword
            if (keywords.Contains(result))
            {
                return true;
            }
            else
            {
                //restore the position if not matched

                _pos = originalPos;
                return false;
            }
        }

        private bool IsId()
        {
            // (LETTER | UNDERSCORE) LETTER_OR_DIGIT*

            string result = "";

            if (!char.IsLetter(_line[_pos]) && !IsMatch('_'))
            {
                return false;
            }

            result += _line[_pos++];

            while (char.IsLetterOrDigit(_line[_pos]) || !IsMatch('_'))
            {
                result += _line[_pos++];
            }

            return result != "true" && result != "false";
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

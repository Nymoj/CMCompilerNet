﻿using System;
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
            int startPos = 0;

            // Skip white spaces before next token
            SkipWhiteSpace();

            // Check to see if file is empty or end of file reached
            if (IsEndLine() && !GetNextLine())
            {
                return null;
            }

            // skip all blank and comment lines
            while (_line.Length == 0 || IsComment() || _commentMode)
            {
                if (!GetNextLine())
                {
                    return null;
                }
            }

            // Check after possibly skipping lines in comment mode is end of file reached
            if (IsEndLine() && !GetNextLine())
            {
                return null;
            }

            // initializing with updated _pos
            startPos = _pos;

            if (IsKeyword())
            {
                token = new Token(TokenType.Keyword, CutTokenFromLine(startPos));
            }
            else if (IsId())
            {
                token = new Token(TokenType.ID, CutTokenFromLine(startPos));
            }
            else if (IsConstant())
            {
                token = new Token(TokenType.Const, CutTokenFromLine(startPos));
            }
            else if (IsStringLiteral())
            {
                token = new Token(TokenType.StringLiteral, CutTokenFromLine(startPos));
            }
            else if (IsSpecialSymbol())
            {
                token = new Token(TokenType.SpecialSymbol, CutTokenFromLine(startPos));
            }
            else if (IsOperator())
            {
                token = new Token(TokenType.Operator, CutTokenFromLine(startPos));
            }
            else
            {
                token = new Token(TokenType.BadToken, CutTokenFromLine(startPos));
                _pos++;
            }

            return token;
        }

        public Token Peek()
        {
            int originalPos = _pos;
            string originalLine = _line;

            Token token = GetNextToken();

            // Check if the line was updated
            // If not, set _pos to its original state
            // If yes, set _pos to the start of the line
            _pos = _line != originalLine ? 0 : originalPos;

            return token;
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

            if (!IsDigit())
            {
                return false;
            }

            while (IsDigit())
            {
                _pos++;
            }

            if (!IsMatch('.'))
            {
                return true;
            }

            _pos++;

            if (!IsDigit())
            {
                _pos = originalPos;
                return false;
            }

            while (IsDigit())
            {
                _pos++;
            }

            return true;
        }

        private bool IsDigit()
        {
            return !IsEndLine() && char.IsDigit(_line[_pos]);
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

        private bool IsStringLiteral()
        {
            int originalPos = _pos;

            if (!IsMatch('\"'))
            {
                return false;
            }

            _pos++;

            while (!IsEndLine())
            {
                if (IsMatch('\"'))
                {
                    _pos++;
                    return true;
                }

                _pos++;
            }

            _pos = originalPos;

            return false;
        }

        private bool IsOperator()
        {
            string op = "";
            op += _line[_pos];

            string[] operators = {
                // arithmetic operators
                "+", "-", "*", "/", "%", "++", "--",
		        // relational operators
		        "==", "!=", ">", "<", ">=", "<=",
		        // logical operators
		        "&&", "||", "!",
		        // bitwise operators
		        "&", "|", "^", "<<", ">>",
		        // assigment operators
		        "=", "+=", "-=", "*=", "/=", "%=",
		        // conditional operator
		        "?",
	        };

            if (!operators.Contains(op))
            {
                return false;
            }

            op += _line[++_pos];

            // if operator with 2 symbols is valid, increase _pos to point to the beggining of next token
            // if not, the operator is one symbol length, and _pos is already increased in previous line
            if (operators.Contains(op))
            {
                _pos++;
            }

            return true;
        }

        private bool IsLetterOrDigit(char symbol)
        {
            return char.IsDigit(symbol) || IsLetter();
        }

        private bool IsEndLine()
        {
            return _stream.EndOfStream || _pos >= _line.Length;
        }

        private bool IsMatch(char symbol)
        {
            return !IsEndLine() && _line[_pos] == symbol;
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

            if (!IsLetter())
            {
                return false;
            }

            while (IsLetter())
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

        private bool IsLetter()
        {
            return !IsEndLine() && (char.IsLetter(_line[_pos]) || IsMatch('_'));
        }

        private bool IsId()
        {
            // (LETTER | UNDERSCORE) LETTER_OR_DIGIT*

            if (!IsLetter())
            {
                return false;
            }

            _pos++;

            while (IsLetterOrDigit())
            {
                _pos++;
            }

            return true;
        }

        private bool IsLetterOrDigit()
        {
            return !IsEndLine() && (IsLetter() || char.IsDigit(_line[_pos]));
        }

        private void SkipWhiteSpace()
        {
            while (IsMatch(' ') || IsMatch('\t'))
            {
                _pos++;
            }
        }

        private bool IsComment()
        {
            return IsSingleLineComment() || IsMultiLineComment();
        }

        private bool IsSingleLineComment()
        {
            // store the start point
            int originalPos = _pos;

            if (_commentMode)
            {
                return false;
            }

            if (!IsMatch('/'))
            {
                return false;
            }

            _pos++;

            if (!IsMatch('/'))
            {
                _pos = originalPos;
                return false;
            }

            _pos++;

            return true;
        }

        private bool IsMultiLineComment()
        {
            // store the start point
            int originalPos = _pos;
            bool starFound = false;

            if (!_commentMode)
            {
                if (!IsMatch('/'))
                {
                    return false;
                }

                _pos++;

                if (!IsMatch('*'))
                {
                    _pos = originalPos;
                    return false;
                }

                _pos++;
                _commentMode = true;

            }

            while (!IsEndLine())
            {
                if (IsMatch('*'))
                {
                    starFound = true;
                }

                _pos++;

                if (IsMatch('/') && starFound)
                {
                    _pos++;

                    _commentMode = false;

                    SkipWhiteSpace();

                    if (IsEndLine() && GetNextLine())
                    {
                        return IsComment();
                    }

                    return IsComment();
                }

                starFound = false;
            }

            return false;
        }

        private bool GetNextLine()
        {
            if (!_stream.EndOfStream)
            {
                _line = _stream.ReadLine();
                _pos = 0;
                SkipWhiteSpace();
                return true;
            }
            return false;
        }

        private bool IsSpecialSymbol()
        {
            char[] specialSymbols = {
                '[', ']', '(', ')', ',', ';', ':', '{', '}',
	        };

            if (!IsEndLine() && !specialSymbols.Contains(_line[_pos]))
            {
                return false;
            }

            _pos++;

            return true;
        }
    }
}

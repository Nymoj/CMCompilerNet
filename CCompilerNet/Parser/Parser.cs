using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CCompilerNet.Lex;

namespace CCompilerNet.Parser
{
    public class Parser
    {
        private string _filePath;
        private Lexer _lexer;
        private Token _currentToken;
        private AST _ast;

        public Parser(Parser other)
        {
            _currentToken = new Token(other._currentToken.Type, other._currentToken.Value);
            _filePath = other._filePath;
        }

        public Parser(string filePath)
        {
            _currentToken = _lexer.GetNextToken();
        }

        public void EatToken()
        {
            _currentToken = _lexer.GetNextToken();
        }

        public bool IsTokenTypeEquals(TokenType tokenType)
        {
            return _currentToken != null && _currentToken.Type == tokenType;
        }

        public bool IsValueEquals(string value)
        {
            return _currentToken != null && _currentToken.Value == value;
        }

        public void ExpectedToken(TokenType tokenType, string tokenValue)
        {
            if (!IsTokenTypeEquals(tokenType) || !IsValueEquals(tokenValue))
            {
                if (_currentToken == null)
                {
                    Error(tokenValue + " expected");
                }

                Error(tokenValue
                    + " expected, but "
                    + _currentToken.Value
                    + " was passed"
                );
            }
        }

        public void ExpectedToken(TokenType tokenType)
        {
            if (!IsTokenTypeEquals(tokenType))
            {
                if (_currentToken == null)
                {
                    Error("const expected");
                }

                Error(string.Format("{0} expected, but {1} was passed", tokenType, _currentToken.Type));
            }
        }

        public void Error(string errorMessage)
        {
            Console.WriteLine("Error: " + errorMessage);
        }

        // program → declList
        private bool CompileProgram()
        {
            var root = new ASTNode("program");
            _ast = new AST(root);

            if (CompileDecList(root))
            {
                return true;
            }

            return false;
        }

        // declList → declList decl | decl   =>   decList → decl decList'   decList' → decl decList' | epsilon
        private bool CompileDecList(ASTNode parent)
        {
            var compileDecList = new ASTNode("decList");
            parent.Add(compileDecList);

            if (CompileDecl(compileDecList))
            {
                if (CompileDecListTag(compileDecList))
                {
                    return true;
                }
            }

            return false; // throw?
        }

        // decList' → decl decList' | epsilon
        private bool CompileDecListTag(ASTNode parent)
        {
            var compileDecListTag = new ASTNode("compileDecListTag");
            parent.Add(compileDecListTag);

            if (CompileDecl(compileDecListTag))
            {
                if (CompileDecListTag(compileDecListTag))
                {
                    return true;
                }
            }

            return true; // not sure. check this.
        }

        private bool CompileDecl(ASTNode parent)
        {
            throw new NotImplementedException();
        }
    }

}

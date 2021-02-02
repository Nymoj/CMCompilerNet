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

                Error(string.Format("{0} expected, but {1} was passed", tokenValue, _currentToken.Value));
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
            System.Environment.Exit(1);
        }

        #region Declarations

        // program -> declList
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

        // declList -> declList decl | decl   =>   decList → decl decList'   decList' → decl decList' | epsilon
        private bool CompileDecList(ASTNode parent)
        {
            ASTNode compileDecList = new ASTNode("decList");
            parent.Add(compileDecList);

            if (CompileDecl(compileDecList))
            {
                if (CompileDeclListTag(compileDecList))
                {
                    return true;
                }
            }

            return false;
        }

        // decList' -> decl decList' | epsilon
        private bool CompileDeclListTag(ASTNode parent)
        {
            ASTNode compileDeclListTag = new ASTNode("declListTag");
            parent.Add(compileDeclListTag);

            if (CompileDecl(compileDeclListTag))
            {
                if (CompileDeclListTag(compileDeclListTag))
                {
                    return true;
                }
            }

            return false;
        }

        // decl -> varDecl | funDecl
        private bool CompileDecl(ASTNode parent)
        {
            ASTNode compileDecl = new ASTNode("decl");
            parent.Add(compileDecl);

            return CompileVarDecl(compileDecl) || CompileFunDecl(compileDecl);
        }
        #endregion

        #region Variable Declarations
        private bool CompileVarDecl(ASTNode parent)
        {
            ASTNode varDecl = new ASTNode("varDecl");
            parent.Add(varDecl);

            if (CompileTypeSpec(varDecl))
            {
                if (CompileVarDeclList(varDecl))
                {
                    if (!IsValueEquals(";"))
                    {
                        return false;
                    }

                    EatToken();
                    return true;
                }
            }

            return false;
        }

        // scopedVarDecl -> static typeSpec varDeclList ; | typeSpec varDeclList ;
        private bool CompileScopedVarDecl(ASTNode parent)
        {
            ASTNode scopedVarDecl = new ASTNode("scopedVarDecl");

            if (IsValueEquals("static"))
            {
                scopedVarDecl.Add(new ASTNode("static", _currentToken));
            }

            if (!CompileTypeSpec(scopedVarDecl))
            {
                return false;
            }

            if (!CompileVarDeclList(scopedVarDecl))
            {
                return false;
            }

            if (!IsValueEquals(";"))
            {
                return false;
            }

            parent.Add(scopedVarDecl);
            return true;
        }

        // varDeclList -> varDeclList, varDeclInit | varDeclInit
        // varDeclList -> varDeclInit varDeclList`
        // varDeclList` -> , varDeclInit varDeclList` | epsilon
        private bool CompileVarDeclList(ASTNode parent)
        {
            ASTNode varDeclList = new ASTNode("varDeclList");
            parent.Add(varDeclList);

            if (CompileVarDeclInit(varDeclList))
            {
                if (CompileVarDeclListTag(varDeclList))
                {
                    return true;
                }
            }

            return false;
        }

        // varDeclList` -> , varDeclInit varDeclList` | epsilon
        private bool CompileVarDeclListTag(ASTNode parent)
        {
            ASTNode varDeclListTag = new ASTNode("varDeclListTag");
            parent.Add(varDeclListTag);

            if (!IsValueEquals(","))
            {
                // epsilon - empty, assuming that the declaration has ended
                return true;
            }

            EatToken();

            if (CompileVarDeclInit(varDeclListTag))
            {
                if (CompileVarDeclListTag(varDeclListTag))
                {
                    return true;
                }
            }

            return false;
        }

        // varDeclInit -> varDeclId | varDeclId : simpleExp
        private bool CompileVarDeclInit(ASTNode parent)
        {
            ASTNode varDeclInit = new ASTNode("varDeclInit");
            parent.Add(varDeclInit);

            if (!CompileVarDeclId(varDeclInit))
            {
                return false;
            }

            // if didn't encounter :, then it's -> varDeclId
            if (!IsValueEquals(":"))
            {
                return true;
            }

            EatToken();

            // else it's -> varDeclId : simpleExp
            return CompileSimpleExp(varDeclInit);
        }

        private bool CompileSimpleExp(ASTNode varDeclInit)
        {
            throw new NotImplementedException();
        }

        // varDeclId -> ID | ID [ NUMCONST ]
        private bool CompileVarDeclId(ASTNode parent)
        {
            ASTNode varDeclId = null;
            // parent.Add(varDeclId);

            if (!IsTokenTypeEquals(TokenType.ID))
            {
                return false;
            }

            // saving the ID
            varDeclId = new ASTNode("varDeclId", _currentToken);
            EatToken();

            // ID
            if (!IsValueEquals("["))
            {
                parent.Add(varDeclId);
                return true;
            }

            EatToken();

            // must be checked further if const is a numconst
            if (!IsTokenTypeEquals(TokenType.Const))
            {
                return false;
            }

            EatToken();

            if (!IsValueEquals("]"))
            {
                return false;
            }

            EatToken();
            parent.Add(varDeclId);
            return true;
        }

        // typeSpec -> int | bool | char
        private bool CompileTypeSpec(ASTNode parent)
        {
            ASTNode typeSpec = null;

            if (IsValueEquals("int") || IsValueEquals("bool") || IsValueEquals("char"))
            {
                typeSpec = new ASTNode("typeSpec", _currentToken);
                parent.Add(typeSpec);

                EatToken();
                return true;
            }
            return false;
        }

        // funDecl -> typeSpec ID ( parms ) stmt | ID ( parms ) stmt
        private bool CompileFunDecl(ASTNode parent)
        {
            ASTNode funDecl = new ASTNode("funDecl");

            CompileTypeSpec(funDecl);

            if (!IsTokenTypeEquals(TokenType.ID))
            {
                return false;
            }

            funDecl.Add(new ASTNode("id", _currentToken));
            EatToken();

            if (!IsValueEquals("("))
            {
                return false;
            }

            EatToken();

            if (!CompileParms(funDecl))
            {
                return false;
            }

            if (!IsValueEquals(")"))
            {
                return false;
            }

            EatToken();

            parent.Add(funDecl);
            return CompileStmt(funDecl);
        }

        private bool CompileStmt(ASTNode parent)
        {
            throw new NotImplementedException();
        }

        // parms -> parmList | epsilon
        private bool CompileParms(ASTNode parent)
        {
            ASTNode parms = new ASTNode("parms");
            // no parameters
            if (IsValueEquals(")"))
            {
                return true;
            }

            if (!CompileParmsList(parms))
            {
                return false;
            }

            parms.Add(parms);
            return true;
        }

        private bool CompileParmsList(ASTNode parent)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

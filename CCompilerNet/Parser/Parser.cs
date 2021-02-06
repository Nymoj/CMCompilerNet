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
        public AST _ast { get; private set; }

        public Parser(Parser other)
        {
            _currentToken = new Token(other._currentToken.Type, other._currentToken.Value);
            _filePath = other._filePath;
        }

        public Parser(string filePath)
        {
            _ast = null;
            _lexer = new Lexer(filePath);
            _currentToken = _lexer.GetNextToken();
        }

        private void EatToken()
        {
            _currentToken = _lexer.GetNextToken();
        }

        private bool IsTokenTypeEquals(TokenType tokenType)
        {
            return _currentToken != null && _currentToken.Type == tokenType;
        }

        private bool IsValueEquals(string value)
        {
            return _currentToken != null && _currentToken.Value == value;
        }

        private void ExpectedToken(TokenType tokenType, string tokenValue)
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
        public bool CompileProgram()
        {
            var root = new ASTNode("program");
            
            if (CompileDeclList(root))
            {
                _ast = new AST(root);
                return true;
            }

            return false;
        }

        // declList -> declList decl | decl   =>   decList → decl decList'   decList' → decl decList' | epsilon
        private bool CompileDeclList(ASTNode parent)
        {
            ASTNode compileDecList = new ASTNode("declList");

            if (CompileDecl(compileDecList))
            {
                if (CompileDeclListTag(compileDecList))
                {
                    parent.Add(compileDecList);
                    return true;
                }
            }

            return false;
        }

        // decList' -> decl decList' | epsilon
        private bool CompileDeclListTag(ASTNode parent)
        {
            ASTNode declListTag = new ASTNode("declListTag");

            // epsilon
            if (!CompileDecl(declListTag))
            {
                return true;
            }

            if (!CompileDeclListTag(declListTag))
            {
                return false;
            }

            parent.Add(declListTag);
            return true;
        }

        // decl -> varDecl | funDecl
        private bool CompileDecl(ASTNode parent)
        {
            ASTNode decl = new ASTNode("decl");

            if (!CompileVarDecl(decl) && !CompileFunDecl(decl))
            {
                return false;
            }

            parent.Add(decl);
            return true;
        }
        #endregion

        #region Variable Declarations
        private bool CompileVarDecl(ASTNode parent)
        {
            ASTNode varDecl = new ASTNode("varDecl");

            if (!CompileTypeSpec(varDecl))
            {
                return false;
            }

            if (!CompileVarDeclList(varDecl))
            {
                return false;
            }

            if (!IsValueEquals(";"))
            {
                return false;
            }

            EatToken();
            parent.Add(varDecl);
            return true;
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

        // varDeclList -> varDeclList , varDeclInit | varDeclInit
        // varDeclList -> varDeclInit varDeclList`
        // varDeclList` -> , varDeclInit varDeclList` | epsilon
        private bool CompileVarDeclList(ASTNode parent)
        {
            ASTNode varDeclList = new ASTNode("varDeclList");

            if (CompileVarDeclInit(varDeclList))
            {
                if (CompileVarDeclListTag(varDeclList))
                {
                    parent.Add(varDeclList);
                    return true;
                }
            }

            return false;
        }

        // varDeclList` -> , varDeclInit varDeclList` | epsilon
        private bool CompileVarDeclListTag(ASTNode parent)
        {
            ASTNode varDeclListTag = new ASTNode("varDeclListTag");
            //parent.Add(varDeclListTag);

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
                    parent.Add(varDeclListTag);
                    return true;
                }
            }

            return false;
        }

        // varDeclInit -> varDeclId | varDeclId : simpleExp
        private bool CompileVarDeclInit(ASTNode parent)
        {
            ASTNode varDeclInit = new ASTNode("varDeclInit");

            if (!CompileVarDeclId(varDeclInit))
            {
                return false;
            }

            // if didn't encounter :, then it's -> varDeclId
            if (!IsValueEquals(":"))
            {
                parent.Add(varDeclInit);
                return true;
            }

            EatToken();

            if (!CompileSimpleExp(varDeclInit))
            {
                return false;
            }

            parent.Add(varDeclInit);
            // else it's -> varDeclId : simpleExp
            return true;
        }

        private bool CompileSimpleExp(ASTNode varDeclInit)
        {
            // TODO
            return true;
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

            varDeclId.Add(new ASTNode("numconst", _currentToken));
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

            if (!CompileParmList(parms))
            {
                return false;
            }

            parms.Add(parms);
            return true;
        }

        // parmList -> parmList ; parmTypeList | parmTypeList
        // parmList -> parmTypeList parmList`
        // parmList` -> ; parmTypeList parmList` | epsilon
        private bool CompileParmList(ASTNode parent)
        {
            ASTNode parmList = new ASTNode("parmList");

            if (!CompileParmTypeList(parmList))
            {
                return false;
            }

            if (!CompileParmListTag(parmList))
            {
                return false;
            }

            parent.Add(parmList);
            return true;
        }

        // parmList` -> ; parmTypeList parmList` | epsilon
        private bool CompileParmListTag(ASTNode parent)
        {
            ASTNode parmListTag = new ASTNode("parmListTag");

            // epsilon
            if (!IsValueEquals(";"))
            {
                return true;
            }

            EatToken();

            if (!CompileParmTypeList(parmListTag))
            {
                return false;
            }

            if (!CompileParmListTag(parmListTag))
            {
                return false;
            }

            parent.Add(parmListTag);
            return true;
        }

        // typeSpec parmIdList
        private bool CompileParmTypeList(ASTNode parent)
        {
            ASTNode parmTypeList = new ASTNode("parmTypeList");

            if (!CompileTypeSpec(parmTypeList))
            {
                return false;
            }

            if (!CompileParmIdList(parmTypeList))
            {
                return false;
            }

            parent.Add(parmTypeList);
            return true;
        }

        // parmIdList -> parmIdList , parmId | parmId
        // parmIdList -> parmId parmIdList`
        // parmIdList` -> , parmId parmIdList` | epsilon
        private bool CompileParmIdList(ASTNode parent)
        {
            ASTNode parmIdList = new ASTNode("parmIdList");

            if (!CompileParmId(parmIdList))
            {
                return false;
            }

            if (!CompileParmIdListTag(parmIdList))
            {
                return false;
            }

            parent.Add(parmIdList);
            return true;
        }

        // parmIdList` -> , parmId parmIdList` | epsilon
        private bool CompileParmIdListTag(ASTNode parent)
        {
            ASTNode parmIdListTag = new ASTNode("parmIdListTag");
            
            // epsilon
            if (!IsValueEquals(";"))
            {
                return true;
            }

            EatToken();

            if (!CompileParmId(parmIdListTag))
            {
                return false;
            }

            if (!CompileParmIdListTag(parmIdListTag))
            {
                return false;
            }

            parent.Add(parmIdListTag);
            return true;
        }

        // parmId -> ID | ID [ ]
        private bool CompileParmId(ASTNode parent)
        {
            ASTNode parmId = null;

            if (!IsTokenTypeEquals(TokenType.ID))
            {
                return false;
            }

            parmId = new ASTNode("parmId", _currentToken);
            EatToken();

            // ID
            if (!IsValueEquals("["))
            {
                parent.Add(parmId);
                return true;
            }

            EatToken();

            if (!IsValueEquals("]"))
            {
                return false;
            }

            parent.Add(parmId);
            return true;
        }
        #endregion
    }
}

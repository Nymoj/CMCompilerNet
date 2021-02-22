﻿using System;
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

            if (!CompileFunDecl(decl) && !CompileVarDecl(decl))
            {
                return false;
            }

            parent.Add(decl);
            return true;
        }
        #endregion

        #region Variable Declarations

        // varDecl -> typeSpec varDeclList ;
        private bool CompileVarDecl(ASTNode parent)
        {
            ASTNode varDecl = new ASTNode("varDecl");

            if (!CompileTypeSpec(varDecl))
            {
                return false;
            }

            EatToken();

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

            EatToken();

            if (!CompileVarDeclList(scopedVarDecl))
            {
                return false;
            }

            if (!IsValueEquals(";"))
            {
                return false;
            }

            EatToken();
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

                //EatToken();
                return true;
            }
            return false;
        }

        // funDecl -> typeSpec ID ( parms ) stmt | ID ( parms ) stmt
        private bool CompileFunDecl(ASTNode parent)
        {
            ASTNode funDecl = new ASTNode("funDecl");

            if (IsValueEquals("int") || IsValueEquals("bool") || IsValueEquals("char"))
            {
                if (_lexer.Peek(2) == null || _lexer.Peek(2).Value != "(")
                {
                    return false;
                }
            }
            else
            {
                if (_lexer.Peek(1) == null || _lexer.Peek(1).Value != "(")
                {
                    return false;
                }
            }

            if (CompileTypeSpec(funDecl))
            {
                // typespec doesn't eat tokens, so here we do it manually
                EatToken();
            }

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

            if (!CompileStmt(funDecl))
            {
                return false;
            }

            parent.Add(funDecl);

            return true;
        }

        // parms -> parmList | epsilon
        private bool CompileParms(ASTNode parent)
        {
            /*ASTNode parms = new ASTNode("parms");
            // no parameters
            if (IsValueEquals(")"))
            {
                return true;
            }

            EatToken();

            if (!CompileParmList(parms))
            {
                return false;
            }

            parms.Add(parms);
            return true;*/

            ASTNode parms = new ASTNode("parms");

            if (!CompileParmList(parms))
            {
                return true;
            }

            parent.Add(parms);
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
            
            // if typespec returned true, type token must be eaten
            // typespec doesn't eat tokens
            EatToken();

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
            if (!IsValueEquals(","))
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

            EatToken();

            parent.Add(parmId);
            return true;
        }
        #endregion

        #region Expression


        // exp -> mutable = exp | mutable += exp | mutable -= exp | mutable *= exp | mutable /= exp | mutable++ | mutable-- | simpleExp
        private bool CompileExp(ASTNode parent)
        {
            ASTNode expression = new ASTNode("expression");
            List<string> mutables = new List<string>{ "=", "+=", "-=", "*=", "/=" };   //list of operators that lead to another expression

            if (CompileMutable(expression))    //checks if its a mutable expression
            {
                if (_currentToken == null)
                {
                    return false;
                }

                if (mutables.Contains(_currentToken.Value)) // checks if mutable -> exp
                {
                    expression.Add(new ASTNode("operator", _currentToken)); //add operator to node of the expression
                    EatToken();

                    if (CompileExp(expression))
                    {
                        parent.Add(expression);
                        return true;
                    }

                    return false;

                }

                if (_currentToken.Value == "++" || _currentToken.Value == "--")
                {
                    expression.Add(new ASTNode("operator", _currentToken));   //add operator to node of the expression

                    parent.Add(expression);
                    EatToken();
                    EatToken();
                    return true;
                }

            }
            if (CompileSimpleExp(expression)) //checks if its a simple expression
            {
                parent.Add(expression);
                return true;
            }

            return false;
        }

        // simpleExp -> simpleExp or andExp | andExp
        // simpleExp -> andExp SimpleExp'
        // simpleExp' -> or andExp simpleExp' | epsilon
        private bool CompileSimpleExp(ASTNode parent)
        {
            ASTNode simpleExp = new ASTNode("simpleExpression");

            if (!CompileAndExp(simpleExp))                        
            { 
                return false;
            }

            if (!CompileSimpleExpTag(simpleExp))
            {
                return false;
            }

            parent.Add(simpleExp);
            return true;
        }
        private bool CompileSimpleExpTag(ASTNode parent)
        {
            ASTNode simpleExpTag = new ASTNode("simpleExpressionTag");

            if (!IsValueEquals("or"))
            {
                return true;
            }

            EatToken();

            if (!CompileAndExp(simpleExpTag))
            {
                return false;
            }

            if (!CompileSimpleExpTag(simpleExpTag))
            {
                return false;
            }

            parent.Add(simpleExpTag);
            return true;
        }

        // AndExp -> andExp and unaryRelExp | unaryRelExp
        // AndExp -> unaryRelExp AndExp'
        // AndExp' -> and unaryRelExp AndExp' | epsilon
        private bool CompileAndExp(ASTNode parent)
        {
            ASTNode andExp = new ASTNode("andExpression");

            if (!CompileUnaryRelExp(andExp))
            {
                return false;
            }

            if (!CompileAndExpTag(andExp))
            {
                return false;
            }

            parent.Add(andExp);
            return true;
        }

        private bool CompileAndExpTag(ASTNode parent)
        {
            ASTNode andExpTag = new ASTNode("andExpressionTag");

            if (!IsValueEquals("and"))
            {
                return true;
            }

            EatToken();

            if (!CompileUnaryRelExp(andExpTag))
            {
                return false;
            }

            if (!CompileAndExpTag(andExpTag))
            {
                return false;
            }

            parent.Add(andExpTag);
            return true;
        }

        //unaryRelExp -> not unaryRelExp | relExp
        private bool CompileUnaryRelExp(ASTNode parent)
        {
            ASTNode unaryRelExp = new ASTNode("unaryRelExpression");

            if (IsValueEquals("not"))
            {
                EatToken();

                if (!CompileUnaryRelExp(unaryRelExp))
                {
                    return false;
                }

                parent.Add(unaryRelExp);
                return true;
            }

            if (CompileRelExp(unaryRelExp))
            {
                parent.Add(unaryRelExp);
                return true;
            }
            return false;   
        }

        //relExp -> MinMaxExp relop MinMaxExp | MinMaxExp
        private bool CompileRelExp(ASTNode parent)
        {
            ASTNode relExp = new ASTNode("relExpression");

            if (!CompileMinMaxExp(relExp))
            {
                return false;
            }

            if (!CompileRelop(relExp))  //if no relop after MinMaxExp then its the 2nd variation
            {
                parent.Add(relExp);
                return true;
            }

            if (!CompileMinMaxExp(relExp))
            {
                return false;
            }

            parent.Add(relExp);
            return true;
        }
        
        //relop -> <= | < | > | >= | == | !=
        private bool CompileRelop(ASTNode parent)
        {
            List<string> operators = new List<string> { "<=", "<", ">", ">=", "==", "!=" }; //list of relop operators
            ASTNode relop = new ASTNode("relop");
            if (!operators.Contains(_currentToken.Value))
            {
                return false;
            }

            relop.Add(new ASTNode("operator", _currentToken));    //add the operator to the relop node
            EatToken();  //move on to the next token after adding it

            parent.Add(relop);  //add to parent node

            return true;

        }
        // minmaxExp -> minmaxExp minmaxOp sumExp | sumExp
        // minmaxExp -> sumExp minmaxExp'
        //minmaxExp' -> minmaxOp sumExp minmaxExp'  | epsilon
        private bool CompileMinMaxExp(ASTNode parent)
        {
            ASTNode minMaxExp = new ASTNode("minMaxExpression");

            if (!CompileSumExp(minMaxExp))
            {
                return false;
            }

            if (!CompileMinMaxExpTag(minMaxExp))
            {
                return false;
            }

            parent.Add(minMaxExp);
            return true;
        }

        private bool CompileMinMaxExpTag(ASTNode parent)
        {
            ASTNode minMaxExpTag = new ASTNode("minMaxExpressionTag");

            if (!CompileMinMaxOp(minMaxExpTag))
            {
                return true;
            }

            if (!CompileSumExp(minMaxExpTag))
            {
                return false;
            }

            if (!CompileMinMaxExpTag(minMaxExpTag))
            {
                return false;
            }

            parent.Add(minMaxExpTag);

            return true;
        }

        //MinMaxOp -> :>: | :<:
        private bool CompileMinMaxOp(ASTNode parent)
        {
            ASTNode minMaxOp = new ASTNode("minMaxOp");

            if (_currentToken.Value != ":>:" && _currentToken.Value != ":<:")
            {
                return false;
            }

            minMaxOp.Add(new ASTNode("operator", _currentToken)); //add operator to node
            EatToken(); //move over to next token 

            parent.Add(minMaxOp); //add node to parent

            return true;
        }

        //SumExp -> SumExp sumOp mulExp | mulExp
        //SumExp -> mulExp SumExp'
        //SumExp' -> sumOp mulExp SumExp' | Epsilon
        private bool CompileSumExp(ASTNode parent)
        {
            ASTNode sumExp = new ASTNode("sumExpression");

            if (!CompileMulExp(sumExp))
            {
                return false;
            }

            if (!CompileSumExpTag(sumExp))
            {
                return false;
            }

            parent.Add(sumExp);

            return true;
        }
        private bool CompileSumExpTag(ASTNode parent)
        {
            ASTNode sumExpTag = new ASTNode("sumExpressionTag");

            if (!CompileSumOp(sumExpTag))
            {
                return true;
            }

            if (!CompileMulExp(sumExpTag))
            {
                return false;
            }

            if (!CompileSumExpTag(sumExpTag))
            {
                return false;
            }

            parent.Add(sumExpTag);

            return true; 
        }

        //sumOp -> + | -
        private bool CompileSumOp(ASTNode parent)
        {
            ASTNode sumOp = new ASTNode("sumOperator");

            if (_currentToken.Value != "+" && _currentToken.Value != "-")
            {
                return false;
            }

            sumOp.Add(new ASTNode("operator", _currentToken));

            EatToken();

            parent.Add(sumOp);

            return true;
        }

        //mulExp -> mulExp mulOp unaryExp | unaryExp
        //mulExp -> unaryExp mulExp'
        //mulExp' -> mulOp unaryExp mulExp' | epsilon 
        private bool CompileMulExp(ASTNode parent)
        {
            ASTNode mulExp = new ASTNode("mulExpression");

            if (!CompileUnaryExp(mulExp))
            {
                return false;
            }

            if (!CompileMulExpTag(mulExp))
            {
                return false;
            }

            parent.Add(mulExp);
            return true;
        }
        private bool CompileMulExpTag(ASTNode parent)
        {
            ASTNode mulExpTag = new ASTNode("mulExpressionTag");

            if (!CompileMulOp(mulExpTag))
            {
                return true;
            }

            if (!CompileUnaryExp(mulExpTag))
            {
                return false;
            }

            if (!CompileMulExpTag(mulExpTag))
            {
                return false;
            }

            parent.Add(mulExpTag);
            return true;  
        }

        //mulOp -> * | / | %
        private bool CompileMulOp(ASTNode parent)
        {
            ASTNode mulOp = new ASTNode("mulOperator");

            if (_currentToken.Value != "*" && _currentToken.Value != "/" && _currentToken.Value != "%")
            {
                return false;
            }

            mulOp.Add(new ASTNode("operator", _currentToken));

            EatToken();

            parent.Add(mulOp);

            return true;
        }

        //unaryExp -> unaryOp unaryExp | factor
        private bool CompileUnaryExp(ASTNode parent)
        {
            ASTNode unaryExp = new ASTNode("unaryExp");

            if (CompileUnaryOperator(unaryExp))
            {
                if (!CompileUnaryExp(unaryExp))
                {
                    return false;
                }

                parent.Add(unaryExp);
                return true;
            }
            if (CompileFactor(unaryExp))
            {
                parent.Add(unaryExp);
                return true;
            }

            return false;
        }

        //unaryOp -> - | * | ?
        private bool CompileUnaryOperator(ASTNode parent)
        {
            ASTNode unaryOp = new ASTNode("unaryOperator");

            if (_currentToken.Value != "-" && _currentToken.Value != "*" && _currentToken.Value != "?")
            {
                return false;
            }

            unaryOp.Add(new ASTNode("operator", _currentToken));

            EatToken();

            parent.Add(unaryOp);

            return true;
        }

        //factor -> immutable | mutable
        private bool CompileFactor(ASTNode parent)
        {
            ASTNode factor = new ASTNode("factor");

            if (CompileImmutable(factor))
            {
                parent.Add(factor);
                return true;
            }

            if (CompileMutable(factor))
            {
                parent.Add(factor);
                EatToken();
                return true;
            }

            return false;
        }

        //immutable ->  ( exp ) | call | constant
        private bool CompileImmutable(ASTNode parent)
        {
            ASTNode immutable = new ASTNode("immutable");

            if (IsValueEquals("("))
            {
                EatToken();

                if (!CompileExp(immutable))
                {
                    return false;
                }

                if (!IsValueEquals(")"))
                {
                    return false;
                }

                EatToken();

                parent.Add(immutable);
                return true;
            }

            if (CompileCall(immutable) || CompileConst(immutable))
            {
                parent.Add(immutable);
                return true;
            }


            return false;
        }

        //mutable -> ID | ID [exp]
        private bool CompileMutable(ASTNode parent)
        {
            ASTNode mutable = new ASTNode("mutable");

            if (!IsTokenTypeEquals(TokenType.ID))
            {
                return false;
            }

            mutable.Add(new ASTNode("ID", _currentToken));
            

            if (!IsValueEquals("["))
            {
                EatToken();
                parent.Add(mutable);
                return true;            //if no [ after id then its not an array
            }
            EatToken();
            EatToken();

            if (!CompileExp(mutable))  //must be an expression between the []
            {
                return false;
            }

            if (!IsValueEquals("]"))
            {
                return false;
            }

            EatToken();

            parent.Add(mutable);

            return true;
        }

        //call -> ID ( args )
        private bool CompileCall(ASTNode parent)
        {
            ASTNode call = new ASTNode("call");

            if (!IsTokenTypeEquals(TokenType.ID))
            {
                return false;
            }

            if (_lexer.Peek(1).Value != "(")
            {
                return false;
            }

            EatToken();
            EatToken();

            if (!CompileArgs(call))
            {
                return false;
            }

            if (!IsValueEquals(")"))
            {
                return false;
            }

            EatToken();

            parent.Add(call);

            return true;

        }

        //args -> argList | epsilon
        private bool CompileArgs(ASTNode parent)
        {
            ASTNode args = new ASTNode("args");

            if (!CompileArgList(args))
            {
                return true;
            }

            parent.Add(args);
            return true;
        }

        //argList -> argList, exp | exp
        //argList -> exp argList'
        //argList -> , exp argList' | epsilon
        private bool CompileArgList(ASTNode parent)
        {
            ASTNode argList = new ASTNode("argList");

            if (!CompileExp(argList))
            {
                return false;
            }

            if (!CompileArgListTag(argList))
            {
                return false;
            }

            parent.Add(argList);
            return true;
        }
        private bool CompileArgListTag(ASTNode parent)
        {
            ASTNode argListTag = new ASTNode("argListTag");

            if (!IsValueEquals(","))
            {
                return true;
            }

            EatToken();

            if (!CompileExp(argListTag))
            {
                return false;
            }

            if (!CompileArgListTag(argListTag))
            {
                return false;
            }

            parent.Add(argListTag);
            return true;
        }

        //const -> NUMCONST | CHARCONST | STRINGCONST | true | false
        private bool CompileConst(ASTNode parent)
        {
            ASTNode constant = new ASTNode("constant");

            if (!IsTokenTypeEquals(TokenType.Const) && !IsTokenTypeEquals(TokenType.StringLiteral))
            {
                return false;
            }

            constant.Add(new ASTNode("const", _currentToken));

            EatToken();

            parent.Add(constant);

            return true;
        }

        #endregion

        #region Statements

        // stmt -> expStmt | compoundStmt | selectStmt | iterStmt | returnStmt | breakStmt
        private bool CompileStmt(ASTNode parent)
        {
            ASTNode stmt = new ASTNode("stmt");

            if (!(
                CompileExpStmt(stmt)
                || CompileCompoundStmt(stmt)
                || CompileSelectStmt(stmt)
                || CompileIterStmt(stmt)
                || CompileReturnStmt(stmt)
                || CompileBreakStmt(stmt)
                ))
            {
                return false;
            }

            parent.Add(stmt);
            return true;
        }

        // expStmt -> exp ; | ;
        private bool CompileExpStmt(ASTNode parent)
        {
            /*ASTNode expStmt = new ASTNode("expStmt");

            // epsilon
            if (IsValueEquals(";"))
            {
                EatToken();
                return true;
            }

            EatToken();

            if (!CompileExp(expStmt))
            {
                return false;
            }

            if (!IsValueEquals(";"))
            {
                return false;
            }

            EatToken();
            parent.Add(expStmt);
            return true;*/
            ASTNode expStmt = new ASTNode("expStmt");

            if (IsValueEquals(";"))
            {
                EatToken();
                return true;
            }

            if (!CompileExp(expStmt) && !IsValueEquals(";"))
            {
                return false;
            }

            if (!IsValueEquals(";"))
            {
                return false;
            }

            EatToken();
            parent.Add(expStmt);
            return true;
        }

        // compoundStmt -> { localDecls stmtList }
        private bool CompileCompoundStmt(ASTNode parent)
        {
            ASTNode compoundStmt = new ASTNode("compoundStmt");

            if (!IsValueEquals("{"))
            {
                return false;
            }

            EatToken();

            if (!CompileLocalDecls(compoundStmt))
            {
                return false;
            }

            if (!CompileStmtList(compoundStmt))
            {
                return false;
            }

            if (!IsValueEquals("}"))
            {
                return false;
            }

            EatToken();
            parent.Add(compoundStmt);
            return true;
        }

        // selectStmt -> if simpleExp then stmt | if simpleExp then stmt else stmt
        private bool CompileSelectStmt(ASTNode parent)
        {
            ASTNode selectStmt = new ASTNode("selectStmt");

            if (!IsValueEquals("if"))
            {
                return false;
            }

            EatToken();

            if (!CompileSimpleExp(selectStmt))
            {
                return false;
            }

            if (!IsValueEquals("then"))
            {
                return false;
            }

            EatToken();

            if (!CompileStmt(selectStmt))
            {
                return false;
            }

            if (!IsValueEquals("else"))
            {
                EatToken();
                parent.Add(selectStmt);
                return true;
            }

            EatToken();

            if (!CompileStmt(selectStmt))
            {
                return false;
            }

            parent.Add(selectStmt);
            return true;
        }

        // iterStmt -> while simpleExp do stmt | for ID = iterRange do stmt
        private bool CompileIterStmt(ASTNode parent)
        {
            ASTNode iterStmt = new ASTNode("iterStmt");

            // while simpleExp do stmt
            if (IsValueEquals("while"))
            {
                EatToken();

                if (!CompileSimpleExp(iterStmt))
                {
                    return false;
                }
                
                if (!IsValueEquals("do"))
                {
                    return false;
                }

                EatToken();

                if (!CompileStmt(iterStmt))
                {
                    return false;
                }

                return true;
            }

            // for ID = iterRange do stmt
            else if (IsValueEquals("for"))
            {
                EatToken();

                if (IsTokenTypeEquals(TokenType.ID))
                {
                    return false;
                }

                // storing the id
                iterStmt.Add(new ASTNode("iterID", _currentToken));

                EatToken();

                if (!CompileIterRange(iterStmt))
                {
                    return false;
                }

                if (!IsValueEquals("do"))
                {
                    return false;
                }

                EatToken();

                if (!CompileStmt(iterStmt))
                {
                    return false;
                }

                parent.Add(iterStmt);
                return true;
            }

            // didn't much any pattern in the production rule
            return false;
        }

        // iterRange -> simpleExp | simpleExp to simpleExp | simpleExp to simpleExp by simpleExp
        private bool CompileIterRange(ASTNode parent)
        {
            ASTNode iterRange = new ASTNode("iterRange");

            if (!CompileSimpleExp(iterRange))
            {
                return false;
            }

            if (!IsValueEquals("to"))
            {
                parent.Add(iterRange);
                return true;
            }

            EatToken();

            if (!CompileSimpleExp(iterRange))
            {
                return false;
            }

            if (!IsValueEquals("by"))
            {
                parent.Add(iterRange);
                return true;
            }

            EatToken();

            if (!CompileSimpleExp(iterRange))
            {
                return false;
            }

            parent.Add(iterRange);
            return true;
        }

        // returnStmt -> return ; | return exp ;
        private bool CompileReturnStmt(ASTNode parent)
        {
            ASTNode returnStmt = new ASTNode("returnStmt");

            if (!IsValueEquals("return"))
            {
                return false;
            }

            EatToken();

            if (!IsValueEquals(";"))
            {
                if (!CompileExp(returnStmt))
                {
                    return false;
                }

                if (!IsValueEquals(";"))
                {
                    return false;
                }

                EatToken();

                parent.Add(returnStmt);
                return true;
            }

            EatToken();

            parent.Add(returnStmt);
            return true;
        }

        private bool CompileBreakStmt(ASTNode parent)
        {
            ASTNode breakStmt = null;

            if (!IsValueEquals("break"))
            {
                return false;
            }

            breakStmt = new ASTNode("breakStmt", _currentToken);
            EatToken();

            if (!IsValueEquals(";"))
            {
                return false;
            }
            EatToken();

            parent.Add(breakStmt);
            return true;
        }

        // stmtList -> stmtList stmt | epsilon
        private bool CompileStmtList(ASTNode parent)
        {
            ASTNode stmtList = new ASTNode("stmtList");

            while (CompileStmt(stmtList)) ;

            if (stmtList.Children.Count() > 0)
            {
                parent.Add(stmtList);
            }

            return true;
        }

        // localDecls -> localDecls scopedVarDecl | epsilon
        private bool CompileLocalDecls(ASTNode parent)
        {
            ASTNode localDecls = new ASTNode("localDecls");

            while (CompileScopedVarDecl(localDecls)) ;

            if (localDecls.Children.Count() > 0)
            {
                parent.Add(localDecls);
            }

            return true;
        }

        #endregion
    }
}

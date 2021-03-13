using CCompilerNet.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

namespace CCompilerNet.CodeGen
{
    public class VMWriter
    {
        private AppDomain _domain;
        private AssemblyBuilder _asmBuilder;
        private ModuleBuilder _moduleBuilder;
        private TypeBuilder _typeBuilder;
        private MethodBuilder _methodBuilder;
        private ILGenerator _mainIL;

        public VMWriter()
        {
            _domain = AppDomain.CurrentDomain;
            _asmBuilder = _domain.DefineDynamicAssembly(
                new AssemblyName("MyASM"), AssemblyBuilderAccess.Save);
            _moduleBuilder = _asmBuilder.DefineDynamicModule(
                "MyASM", "output.exe");
            _typeBuilder = _moduleBuilder.DefineType("Program",
                TypeAttributes.Class | TypeAttributes.Public);
            _methodBuilder = _typeBuilder.DefineMethod(
                "Main", MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static,
                typeof(int), new Type[] { typeof(string[]) });
            _mainIL = _methodBuilder.GetILGenerator();
            
        }

        public void GenerateCode(AST ast, string outputPath)
        {
            CodeWriteFunc(ast.Root);
            _typeBuilder.CreateType();
            _asmBuilder.SetEntryPoint(_methodBuilder, PEFileKinds.ConsoleApplication);
            File.Delete("output.exe");
            _asmBuilder.Save("output.exe");
        }

        private void Push(string value)
        {
            _mainIL.Emit(OpCodes.Ldc_I4, int.Parse(value));
        }

        public void CodeWriteFunc(ASTNode ast)
        {
            if (ast.Tag == "program")
            {
                CodeWriteFunc(ast.Children[0]); //code write of first decList

            }
            if (ast.Tag == "declList" || ast.Tag == "declListTag")   //goes over list of all functions and calls every one of them
            {
                foreach (ASTNode child in ast.Children)
                {
                    CodeWriteFunc(child);
                }
            }
            if (ast.Tag == "decl")
            {
                CodeWriteFunc(ast.Children[0]);
            }
            if (ast.Tag == "funDecl")   //if current node is a function
            {
                foreach (ASTNode child in ast.Children)  //goes over each child until it reaches the statements
                {
                    if (child.Tag == "stmt" && child.Children[0].Children.Count != 0) //checks if statement and if func isnt empty
                    {
                        foreach (ASTNode _child in child.Children[0].Children[0].Children)   //goes over all statements
                        {
                            if (_child.Children[0].Tag == "ExpStmt")
                            {
                                CodeWriteExp(_child);

                            }
                            if (_child.Children[0].Tag == "returnStmt")
                            {
                                CodeWriteExp(_child.Children[0].Children[1]);   //sends the expression after the return
                                _mainIL.Emit(OpCodes.Ret);

                            }


                        }

                    }
                }

            }
            
        }

        public void CodeWriteExp(ASTNode exp)
        {
            if (exp.Tag == "constant" || exp.Tag == "mutable")
            {
                Push(exp.Children[0].Token.Value);
            }

            if (exp.Tag == "operator")
            {
                // Console.WriteLine(exp.Children[0].Token.Value);
                switch(exp.Children[0].Token.Value)
                {
                    case "+":
                        _mainIL.Emit(OpCodes.Add);
                        break;
                    case "-":
                        _mainIL.Emit(OpCodes.Sub);
                        break;
                    case "*":
                        _mainIL.Emit(OpCodes.Mul);
                        break;
                    case "/":
                        _mainIL.Emit(OpCodes.Div);
                        break;
                }
            }

            if (exp.Children.Count > 1)
            {
                
                if (exp.Tag == "mulExpression" || exp.Tag == "sumExpression")
                {
                    CodeWriteExp(exp.Children[0]);    //push 1st exp

                    CodeWriteExp(exp.Children[1]);        //code generation of tag

                }

                if (exp.Tag == "mulExpressionTag" || exp.Tag == "sumExpressionTag")
                {
                    foreach (ASTNode child in exp.Children)
                    {
                        CodeWriteExp(child);
                    }
                }

                if (exp.Tag == "unaryExp")
                {
                    CodeWriteExp(exp.Children[0]); //push exp

                    Console.WriteLine(exp.Children[1].Children[0].Token.Value); //push operator
                }

                if (exp.Tag == "call")
                {
                    if (exp.Children[1].Children[0].Children.Any()) //checks if argument list is empty
                    {
                        CodeWriteExp(exp.Children[1].Children[0].Children[0]); //first argument of function

                        if (exp.Children[1].Children[0].Children.Count > 1)     //checks if theres more than 1 arguments resulting in arg tag
                        {
                            foreach (ASTNode child in exp.Children[1].Children[0].Children[1].Children) //goes over all arg list tag members
                            {
                                CodeWriteExp(child);
                            }
                        }
                    }
                    Console.WriteLine("call " + exp.Children[0].Token.Value);  //call func
                }

                
            }

            if (exp.Children.Count == 1)
            {
                CodeWriteExp(exp.Children[0]);     //move over to next member
            }
        }
    }
}

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
            _asmBuilder = _domain.DefineDynamicAssembly(new AssemblyName("MyASM"), AssemblyBuilderAccess.Save);
            _moduleBuilder = _asmBuilder.DefineDynamicModule("MyModule");
            // defining main static class Program
            _typeBuilder = _moduleBuilder.DefineType(
                "Program",
                TypeAttributes.Class | TypeAttributes.Public
                );

            // defining Main function
            _methodBuilder = _typeBuilder.DefineMethod("Main",
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                typeof(int),
                new Type[] { typeof(string) }
            );

            _mainIL = _methodBuilder.GetILGenerator();

            _mainIL.Emit(OpCodes.Ret);

            _typeBuilder.CreateType();
            _asmBuilder.SetEntryPoint(_methodBuilder, PEFileKinds.ConsoleApplication);
            File.Delete("test.exe");
            _asmBuilder.Save("test.exe");

            Console.WriteLine("Created successfuly!");
            /*const string ASSEMBLY_NAME = "IL_Test";
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName(ASSEMBLY_NAME), AssemblyBuilderAccess.Save);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
                ASSEMBLY_NAME, "test.exe");
            TypeBuilder typeBuilder = moduleBuilder.DefineType("Program",
                TypeAttributes.Class | TypeAttributes.Public);
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                "Main", MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static,
                typeof(void), new Type[] { typeof(string[]) });
            ILGenerator gen = methodBuilder.GetILGenerator();

            gen.Emit(OpCodes.Ldstr, "Hello, World!");
            gen.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadKey", new Type[] { typeof(bool) }));
            gen.Emit(OpCodes.Pop);
            typeBuilder.CreateType();
            assemblyBuilder.SetEntryPoint(methodBuilder, PEFileKinds.ConsoleApplication);
            File.Delete("test.exe");
            assemblyBuilder.Save("test.exe");*/
        }

        public void GenerateCode(AST ast, string outputPath)
        {
            //CodeWriteExp(ast.Root);

            _mainIL.Emit(OpCodes.Ret);

            _typeBuilder.CreateType();
            _asmBuilder.SetEntryPoint(_methodBuilder, PEFileKinds.ConsoleApplication);
            File.Delete("test.exe");
            _asmBuilder.Save("test.exe");

            Console.WriteLine("Created successfuly!");
        }

        private void Push(string value)
        {
            _mainIL.Emit(OpCodes.Ldc_I4, int.Parse(value));
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

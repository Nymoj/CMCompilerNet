using CCompilerNet.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using CCompilerNet.Lex;

namespace CCompilerNet.CodeGen
{
    public class VMWriter
    {
        private AppDomain _domain;
        private AssemblyBuilder _asmBuilder;
        private ModuleBuilder _moduleBuilder;
        private TypeBuilder _typeBuilder;

        private MethodBuilder _methodBuilder;
        private ILGenerator _currILGen;

        // global scope table
        public SymbolTable SymbolTable { get; set; }
        // function table
        public FunctionTable FunctionTable { get; }

        public VMWriter()
        {
            _domain = AppDomain.CurrentDomain;
            _asmBuilder = _domain.DefineDynamicAssembly(
                new AssemblyName("MyASM"), AssemblyBuilderAccess.Save);
            _moduleBuilder = _asmBuilder.DefineDynamicModule(
                "MyASM", "output.exe", true);
            _typeBuilder = _moduleBuilder.DefineType("Program",
                TypeAttributes.Class | TypeAttributes.Public);
            /*_methodBuilder = _typeBuilder.DefineMethod(
                "Main", MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static,
                typeof(int), new Type[] { typeof(string[]) });
            _currILGen = _methodBuilder.GetILGenerator();*/

            SymbolTable = new SymbolTable(null);
            FunctionTable = new FunctionTable(_typeBuilder);
        }

        private void Push(char value)
        {
            _currILGen.Emit(OpCodes.Ldc_I4, value);
        }

        private void Push(int value)
        {
            _currILGen.Emit(OpCodes.Ldc_I4, value);
        }

        private void Push(bool value)
        {
            _currILGen.Emit(OpCodes.Ldc_I4, value ? 1 : 0);
        }

        private void Push(string id)
        {
            Symbol symbol = SymbolTable.GetSymbol(id);

            if (symbol == null)
            {
                Console.Error.WriteLine($"Error: {id} is not declared.");
                Environment.Exit(-1);
            }

            _currILGen.Emit(OpCodes.Ldloc, symbol.LocalBuilder.LocalIndex);
        }

        public void CodeWriteFunction(ASTNode root)
        {
            string id = SemanticHelper.GetFunctionId(root);
            //ASTNode stmt = SemanticHelper.GetFunctionStmt(root);

            _methodBuilder = FunctionTable.GetFunctionSymbol(id).MethodBuilder;
            _currILGen = _methodBuilder.GetILGenerator();

            /*SymbolTable = SymbolTable.StartSubRoutine();

            foreach (ASTNode child in stmt.Children)
            {
                //CodeWriteStmt(child);
            }

            SymbolTable = SymbolTable.GetNext();*/
        }

        public void Save(string path)
        {
            _typeBuilder.CreateType();

            _asmBuilder.SetEntryPoint(_methodBuilder, PEFileKinds.ConsoleApplication);
            File.Delete(path);
            _asmBuilder.Save(path);
        }

        public void CodeWriteReturnStmt(ASTNode root)
        {
            if (root.Children.Count > 1)
            {
                CodeWriteExp(root.Children[1]);   //sends the expression after the return
            }
            _currILGen.Emit(OpCodes.Ret);
        }

        public void CodeWriteStmtList(ASTNode root)
        {
            // iterating through the statements
            foreach(ASTNode child in root.Children)
            {
                CodeWriteStmt(child);
            }
        }

        public void CodeWriteStmt(ASTNode root)
        {
            switch(root.Children[0].Tag)
            {
                case "returnStmt":
                    CodeWriteReturnStmt(root.Children[0]);
                    break;
                case "selectStmt":
                    CodeWriteSelectStmt(root.Children[0]);
                    break;
                case "expStmt":
                    CodeWriteExp(root.Children[0]);
                    break;
                case "compoundStmt":
                    CodeWriteCompoundStmt(root.Children[0]);
                    break;
            }
        }

        public void CodeWriteCompoundStmt(ASTNode root)
        {
            SymbolTable = SymbolTable.StartSubRoutine();
            // if localDecls exists then stmt is the second child,
            // otherwise stmts are the only children
            int index = 0;

            if (root.Children[0].Tag == "localDecls")
            {
                foreach (ASTNode child in root.Children[0].Children)
                {
                    AddSymbolsFromScopedVarDecl(child);
                }
                index = 1;
            }
            
            foreach (ASTNode child in root.Children[index].Children)
            {
                CodeWriteStmt(child);
            }

            SymbolTable = SymbolTable.GetNext();
        }

        public void CodeWriteIterStmt(ASTNode root)
        {
            
        }
        public void CodeWriteSelectStmt(ASTNode root)
        {
            
            // checking if the root is just a single if
            // with else
            if (root.Children.Count > 2)
            {
                CodeWriteSimpleExp(root.Children[0]);
                Label toEnd = _currILGen.DefineLabel();
                Label toElse = _currILGen.DefineLabel();

                // branching to else statements if the condition is false
                _currILGen.Emit(OpCodes.Brfalse, toElse);

                // translating the statements inside the if
                CodeWriteStmt(root.Children[1]);
                // finishing the if statement
                _currILGen.Emit(OpCodes.Br, toEnd);

                _currILGen.MarkLabel(toElse);
                // translating the statements inside else
                CodeWriteStmt(root.Children[2]);
                _currILGen.MarkLabel(toEnd);
            }
            // without else
            else
            {
                
                CodeWriteSimpleExp(root.Children[0]);
                Label toEnd = _currILGen.DefineLabel();
                _currILGen.Emit(OpCodes.Brfalse, toEnd);
                CodeWriteStmt(root.Children[1]);
                _currILGen.MarkLabel(toEnd);
                //CodeWriteSimpleExp(root.Children[0]);

                /*var toEnd = _currILGen.DefineLabel();
                _currILGen.Emit(OpCodes.Brfalse, toEnd);
                _currILGen.Emit(OpCodes.Ldstr, "Hello");
                _currILGen.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
                _currILGen.MarkLabel(toEnd);*/
            }
        }

        public void CodeWriteWhileLoop(ASTNode root)
        {
            Label toLoopTop = _currILGen.DefineLabel();
            Label toCondition = _currILGen.DefineLabel();

            _currILGen.Emit(OpCodes.Br, toCondition);
            
            _currILGen.MarkLabel(toLoopTop);
            // translating the statements
            CodeWriteStmt(root.Children[3]);
            _currILGen.MarkLabel(toCondition);
            // translating the condition
            CodeWriteSimpleExp(root.Children[1]);
            _currILGen.Emit(OpCodes.Brtrue, toLoopTop);
        }

        public string CodeWriteExp(ASTNode exp)
        {
            if (exp.Children.Count == 3 && exp.Children[1].Token != null)
            {
                
                if (exp.Children[2].Children.Count < 2)
                {
                    Symbol symbol = SymbolTable.GetSymbol(GetID(exp.Children[0]));
                    string type;

                    if (symbol == null)
                    {
                        Console.Error.WriteLine($"Error: {exp.Children[0].Token.Value} is not declared.");
                        Environment.Exit(-1);
                    }


                        // possibly will be removed
                        // the condition is checked in the parser
                        if (symbol == null)
                        {
                            Console.Error.WriteLine($"Error: {exp.Children[0].Token.Value} is not declared.");
                            Environment.Exit(-1);
                        }

                    type = CodeWriteExp(exp.Children[2]);


                    if (type != symbol.Type)
                    {
                        Console.Error.WriteLine($"Error: type missmatch");
                        Environment.Exit(-1);
                    }

                    switch (exp.Children[1].Token.Value)
                    {
                        case "=":
                            _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
                            break;
                        case "+=":
                            Push(GetID(exp.Children[0]));
                            _currILGen.Emit(OpCodes.Add);
                            _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
                            break;
                        case "-=":
                            //order of push is important so pushes the 2nd part again and pops the remaining one 
                            Push(GetID(exp.Children[0]));
                            CodeWriteExp(exp.Children[2]);
                            _currILGen.Emit(OpCodes.Sub);                                      
                            _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
                            _currILGen.Emit(OpCodes.Pop);
                            break;
                        case "*=":
                            Push(GetID(exp.Children[0]));
                            _currILGen.Emit(OpCodes.Mul);
                            _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
                            break;
                        case "/=":
                            //order of push is important so pushes the 2nd part again and pops the remaining one 

                            Push(GetID(exp.Children[0]));
                            CodeWriteExp(exp.Children[2]);
                            _currILGen.Emit(OpCodes.Div);
                            _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
                            _currILGen.Emit(OpCodes.Pop);
                            break;

                    }

                    return type;
                }
                else
                {
                    Symbol symbol = SymbolTable.GetSymbol(GetID(exp.Children[0]));
                    string type;

                    if (symbol == null)
                    {
                        Console.Error.WriteLine($"Error: {exp.Children[0].Token.Value} is not declared.");
                        Environment.Exit(-1);
                    }

                    type = CodeWriteExp(exp.Children[2]);

                    Push(GetID(exp.Children[2]));

                    if (type != symbol.Type)
                    {
                        Console.Error.WriteLine($"Error: type missmatch");
                        Environment.Exit(-1);
                    }

                    switch (exp.Children[1].Token.Value)
                    {
                        case "=":
                            _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
                            break;
                        case "+=":
                            Push(GetID(exp.Children[0]));
                            _currILGen.Emit(OpCodes.Add);
                            _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
                            break;
                        case "-=":
                            //order of push is important so pushes the 2nd part again and pops the remaining one 

                            Push(GetID(exp.Children[0]));
                            CodeWriteExp(exp.Children[2]);
                            _currILGen.Emit(OpCodes.Sub);
                            _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
                            _currILGen.Emit(OpCodes.Pop);
                            break;
                        case "*=":
                            Push(GetID(exp.Children[0]));
                            _currILGen.Emit(OpCodes.Mul);
                            _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
                            break;
                        case "/=":
                            //order of push is important so pushes the 2nd part again and pops the remaining one 

                            Push(GetID(exp.Children[0]));
                            CodeWriteExp(exp.Children[2]);
                            _currILGen.Emit(OpCodes.Div);
                            _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
                            _currILGen.Emit(OpCodes.Pop);
                            break;

                    }

                    return type;
                }


            }

            if (exp.Children.Count == 2 && exp.Children[1].Token != null)
            {
                string op = exp.Children[1].Token.Value;
                if (op == "++" || op == "--")
                {
                    Symbol symbol = SymbolTable.GetSymbol(GetID(exp.Children[0]));
                    if (symbol.Type != "int")
                    {
                        Console.Error.WriteLine($"Error: type missmatch");
                        Environment.Exit(-1);
                    }

                    Push(GetID(exp.Children[0]));
                    Push(1);

                    if (op == "++")
                    {
                        _currILGen.Emit(OpCodes.Add);

                    }
                    else
                    {
                        _currILGen.Emit(OpCodes.Sub);
                    }

                    _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);

                    return "int";

                }
            }

            return CodeWriteSimpleExp(exp);
        }
        public string CodeWriteSimpleExp(ASTNode exp)
        {

            if (exp.Tag == "constant" || exp.Tag == "mutable")
            {
                string value = exp.Children[0].Token.Value;

                if (exp.Tag == "constant")
                {
                    if (value == "false" || value == "true")
                    {
                        Push(bool.Parse(value));
                        return "bool";
                    }

                    if (value.Contains("'"))
                    {
                        Push(value.ElementAt(1));
                        return "char";
                    }

                    Push(int.Parse(value));
                    return "int";
                }
                else
                {
                    // pushing the id
                    Push(value);
                    return SymbolTable.GetSymbol(value).Type;
                }
            }

            if (exp.Tag == "call")
            {
                FunctionSymbol func = FunctionTable.GetFunctionSymbol(exp.Children[0].Token.Value);

                if (func == null)
                {
                    Console.Error.WriteLine($"Error: function does not exist.");
                    Environment.Exit(-1);
                }

                if (func.ParmTypeList != null) //checks if argument list is empty
                {
                    if (exp.Children.Count == 1)
                    {
                        Console.Error.WriteLine($"Error: missing parameters when calling a function.");
                        Environment.Exit(-1);
                    }

                    /*CodeWriteExp(exp.Children[1].Children[0].Children[0]); //first argument of function

                    if (exp.Children[1].Children[0].Children.Count > 1)     //checks if theres more than 1 arguments resulting in arg tag
                    {
                        foreach (ASTNode child in exp.Children[1].Children[0].Children[1].Children) //goes over all arg list tag members
                        {
                            CodeWriteExp(child);
                        }
                    }
                    */

                    for (int i = 0; i < exp.Children[1].Children.Count; i++)
                    {
                        if (CodeWriteExp(exp.Children[1].Children[i]) != func.ParmTypeList[i])
                        {
                            Console.Error.WriteLine($"Error: parameter type missmatch");
                            Environment.Exit(-1);
                        }
                    }
                }
                _currILGen.Emit(OpCodes.Call, func.MethodBuilder);  //call func

                return func.Type;
            }

            if (exp.Tag == "operator")
            {
                // Console.WriteLine(exp.Children[0].Token.Value);
                switch (exp.Children[0].Token.Value)
                {
                    case "+":
                        _currILGen.Emit(OpCodes.Add);
                        break;
                    case "-":
                        _currILGen.Emit(OpCodes.Sub);
                        break;
                    case "*":
                        _currILGen.Emit(OpCodes.Mul);
                        break;
                    case "/":
                        _currILGen.Emit(OpCodes.Div);
                        break;
                }

                return "operator";
            }

            if (exp.Tag == "unaryOperator")
            {
                switch (exp.Children[0].Token.Value)
                {
                    case "-":
                        _currILGen.Emit(OpCodes.Neg);
                        break;
                    case "not":
                        _currILGen.Emit(OpCodes.Ldc_I4_0);
                        _currILGen.Emit(OpCodes.Ceq);
                        break;

                }
            }

            if (exp.Children.Count > 1)
            {
                if (exp.Tag == "andExpression")
                {
                    string type = CodeWriteExp(exp.Children[0]);

                    for (int i = 1; i < exp.Children.Count; i++)
                    {
                        if (type != CodeWriteExp(exp.Children[i]))
                        {
                            Console.Error.WriteLine($"Error: type missmatch");
                            Environment.Exit(-1);
                        }
                        _currILGen.Emit(OpCodes.And);
                    }
                    return type;
                }

                if (exp.Tag == "simpleExpression")
                {
                    string type = CodeWriteExp(exp.Children[0]);

                    for (int i = 1; i < exp.Children.Count; i++)
                    {
                        if (type != CodeWriteExp(exp.Children[i]))
                        {
                            Console.Error.WriteLine($"Error: type missmatch");
                            Environment.Exit(-1);
                        }
                        _currILGen.Emit(OpCodes.Or);
                    }
                    return type;
                }


                if (exp.Tag == "relExpression")
                {
                    //push both members of the rel expression
                    CodeWriteExp(exp.Children[0]);
                    CodeWriteExp(exp.Children[2]);

                    switch (exp.Children[1].Children[0].Token.Value) // switch case of rel expression operator
                    {
                        case "==":
                            _currILGen.Emit(OpCodes.Ceq);
                            break;
                        case "!=":
                            _currILGen.Emit(OpCodes.Ceq);
                            _currILGen.Emit(OpCodes.Ldc_I4_0);
                            _currILGen.Emit(OpCodes.Ceq);
                            break;
                        case ">":
                            _currILGen.Emit(OpCodes.Cgt);
                            break;
                        case "<":
                            _currILGen.Emit(OpCodes.Clt);
                            break;
                        case ">=":
                            _currILGen.Emit(OpCodes.Clt);
                            _currILGen.Emit(OpCodes.Ldc_I4_0);
                            _currILGen.Emit(OpCodes.Ceq);
                            break;
                        case "<=":
                            _currILGen.Emit(OpCodes.Cgt);
                            _currILGen.Emit(OpCodes.Ldc_I4_0);
                            _currILGen.Emit(OpCodes.Ceq);
                            break;

                    }

                    return "bool";
                }

                if (exp.Tag == "mulExpression" || exp.Tag == "sumExpression")
                {
                    string type = CodeWriteExp(exp.Children[0]);    //push 1st exp

                    if (!CodeWriteTag(exp.Children[1], type))        //code generation of tag
                    {
                        Console.Error.WriteLine($"Error: types mismatch");
                        Environment.Exit(-1);
                    }

                    return type;
                }



                /*if (exp.Tag == "mulExpressionTag" || exp.Tag == "sumExpressionTag")
                {
                    foreach (ASTNode child in exp.Children)
                    {
                        CodeWriteExp(child);
                    }
                }*/

                if (exp.Tag == "unaryExp")
                {
                    string type = CodeWriteExp(exp.Children[0]); //push exp

                    CodeWriteExp(exp.Children[1]); // pushes operator  

                    return type;
                }

                if (exp.Tag == "unaryRelExpression")
                {
                    string type = CodeWriteExp(exp.Children[1]); //push exp

                    _currILGen.Emit(OpCodes.Ldc_I4_0);
                    _currILGen.Emit(OpCodes.Ceq);
                }

                
                

            }

            if (exp.Children.Count == 1)
            {
                return CodeWriteExp(exp.Children[0]);     //move over to next member
            }

            return null;
        }

        public bool CodeWriteTag(ASTNode exp, string type)
        {
            string currType;
            foreach (ASTNode child in exp.Children)
            {
                if (child.Tag == "mulExpressionTag" || child.Tag == "sumExpressionTag")
                {
                    CodeWriteTag(child, type);
                }
                else
                {
                    currType = CodeWriteExp(child);

                    if (currType != "operator" && type != currType)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void CodeWriteScopedVarDecl(ASTNode root, string name, string type)
        {
            Symbol symbol = SymbolTable.GetSymbol(name);

            if (symbol == null)
            {
                Console.Error.WriteLine($"Error: {name} is not declared.");
                Environment.Exit(-1);
            }

            if (symbol.Type != type)
            {
                Console.Error.WriteLine($"Error: type missmatch");
                Environment.Exit(-1);
            }

            if (root.Children.Count > 1)
            {
                
            }
            else
            {
                _currILGen.Emit(OpCodes.Stloc, symbol.LocalBuilder.LocalIndex);
            }
        }

        public void AddSymbolsFromScopedVarDecl(ASTNode root)
        {
            string type = "";
            // starting as local kind
            Kind attribute = Kind.LOCAL;
            // points to the varDeclInit in children
            // depends on the number of children, assuming the number is 2 at the beginning
            int startIndex = 1;

            // if there is an attribute, the count of children will be 3 (static, typeSpec varDeclList)
            if (root.Children.Count > 2)
            {
                // changing the start index to 2 because the number of children is 3
                startIndex = 2;
                // changing the attribute to be static
                attribute = Kind.STATIC;
            }

            type = root.Children[startIndex - 1].Token.Value;

            // iterating through the IDs and adding them to the symbol table
            foreach (ASTNode child in root.Children[startIndex].Children)
            {
                if (child.Children[0].Children.Count > 1)
                {
                    SymbolTable.Define(child.Children[0].Children[0].Token.Value, type, attribute, _currILGen.DeclareLocal(ConvertToType(type, true)));

                    // init the array
                    _currILGen.Emit(OpCodes.Ldc_I4, int.Parse(child.Children[0].Children[1].Token.Value));
                    _currILGen.Emit(OpCodes.Newarr, ConvertToType(type, false));
                    _currILGen.Emit(OpCodes.Stloc, SymbolTable.GetSymbol(child.Children[0].Children[0].Token.Value).Index);
                }
                else
                {
                    SymbolTable.Define(child.Children[0].Children[0].Token.Value, type, attribute, _currILGen.DeclareLocal(ConvertToType(type, false)));
                }

                // check if initialization is needed
                // the second child of varDeclInit is always the value
                if (child.Children.Count > 1)
                {
                    CodeWriteScopedVarDecl(child.Children[0], child.Children[0].Children[0].Token.Value, CodeWriteExp(child.Children[1]));
                }
            }
        }

        public static Type ConvertToType(string type, bool isArrayType)
        {
            if (!isArrayType)
            {
                switch (type)
                {
                    case "int":
                        return typeof(Int32);
                    case "bool":
                        return typeof(Boolean);
                    case "char":
                        return typeof(Char);
                    default:
                        return null;
                }
            }
            else
            {
                switch (type)
                {
                    case "int":
                        return typeof(Int32[]);
                    case "bool":
                        return typeof(Boolean[]);
                    case "char":
                        return typeof(Char[]);
                    default:
                        return null;
                }
            }
        }
        public static Type[] ConvertToType(string[] types)
        {
            if (types == null || types.Length == 0)
            {
                // no types to convert
                return null;
            }
            Type[] result = new Type[types.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ConvertToType(types[i], types[i].Contains('['));
            }

            return result;
        }

        public string GetID(ASTNode root)
        {
            if (root.Tag == "mutable")
            {
                return root.Children[0].Token.Value;
            }

            if (root.Tag == "constant")
            {
                return null;
            }

            return GetID(root.Children[0]);
        }
    }
}

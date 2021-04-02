using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCompilerNet.CodeGen;
using CCompilerNet.Lex;
using CCompilerNet.Parser;

namespace CCompilerNet
{
    class Program
    {
        static void Main(string[] args)
        {
            string output = "";
            string path = "";

            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine(args[i]);

                if (args[i].Contains("-output="))
                {
                    output = args[i].Replace("-output=", "");
                }

                if (args[i].Contains(".c"))
                {
                    path = args[i];
                }

            }

            output = output == "" ? path.Replace(".c", ".exe") : output;   //if output is empty save the .exe file at the same path as the .c file

            StreamWriter outputFile = new StreamWriter("output.xml");
            Parser.Parser parser = new Parser.Parser(args[0], Path.GetFileName(output));
            parser.CompileProgram();

            AST ast = parser._ast;
            parser._vm.Save(Path.GetFileName(output));
            File.Move(Path.GetFileName(output), output);
            outputFile.Write(ast?.ToString());
            outputFile.Close(); 
        }
    }
}

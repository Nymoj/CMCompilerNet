using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CCompilerNet.Lex;
using CCompilerNet.Parser;

namespace CCompilerNet
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamWriter outputFile = new StreamWriter("E:/output.xml");
            Parser.Parser parser = new Parser.Parser(args[0]);
            parser.CompileProgram();

            AST ast = parser._ast;

            outputFile.Write(ast?.ToString());
            outputFile.Close();

            /*if (args.Count() < 1)
            {
                Console.WriteLine("No input file provided");
                return;
            }

            Lexer lex = new Lexer(args[0]);

            Console.WriteLine("------" + lex.Peek(3) + "----------");

            while (lex.Peek(1) != null)
            {
                Console.WriteLine(lex.GetNextToken());
            }*/
        }
    }
}

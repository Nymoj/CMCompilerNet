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
            StreamWriter outputFile = new StreamWriter("E:\\output.xml");
            Parser.Parser parser = new Parser.Parser(args[0]);
            parser.CompileProgram();

            AST ast = parser._ast;

            outputFile.Write(ast?.ToString());
            outputFile.Close();
        }
    }
}

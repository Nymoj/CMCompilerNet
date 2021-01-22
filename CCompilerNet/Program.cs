using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CCompilerNet.Lex;

namespace CCompilerNet
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer lexer = new Lexer(args[0]);

            while (lexer.Peek() != null)
            {
                Console.WriteLine(lexer.GetNextToken());
            }
        }
    }
}

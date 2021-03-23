using CCompilerNet.Lex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCompilerNet.Parser
{
    public static class SemanticHelper
    {
        public static string GetFunctionType(ASTNode root)
        {
            Token type = root.Children[0].Token;

            if (type.Value != "int" && type.Value != "bool" && type.Value != "char")
            {
                return "void";
            }

            return type.Value;
        }

        public static string GetFunctionId(ASTNode root)
        {
            if (GetFunctionType(root) != "void")
            {
                return root.Children[1].Token.Value;
            }

            return root.Children[0].Token.Value;
        }

        public static List<string> GetFunctionParmTypes(ASTNode root)
        {
            ASTNode parmList = null;
            List<string> result = new List<string>();

            if (GetFunctionType(root) == "void")
            {
                parmList = root.Children[1].Children[0];
            }
            else
            {
                if (root.Children.Count < 3)
                {
                    return null;
                }
                parmList = root.Children[2].Children[0];
            }

            // iterating through parmTypeLists
            foreach (ASTNode child in parmList.Children)
            {
                result.Add(child.Children[0].Token.Value);
            }

            return result;
        }
    }
}

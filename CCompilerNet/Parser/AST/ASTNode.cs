using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CCompilerNet.Lex;

namespace CCompilerNet.Parser
{
    public class ASTNode
    {
        /* Public Properties */
        public string Tag { get; set; }
        public List<ASTNode> Children { get; private set; }
        public Token Token { get; set; }
                                                                                    
        /* Constructors */
        public ASTNode(string tag, Token token = null)
        {
            Children = new List<ASTNode>();
            Tag = tag;
            Token = token;
        }

        /* Public Methods */
        /// <summary>
        /// Adds a child node to the current node
        /// </summary>
        /// <param name="node">Node to be added</param>
        public void Add(ASTNode node)
        {
            Children.Add(node);
        }

        public static string Print(ASTNode node, int level)
        {
            string tabs = new string('\t', level);

            string result = "";

            result += tabs + "<" + node.Tag + ">\n";

            if (node.Token != null)
            {
                result += tabs + '\t' + node.Token.ToString() + '\n';
            }

            foreach (ASTNode child in node.Children)
            {
                result += ASTNode.Print(child, level + 1);
            }

            result += tabs + "</" + node.Tag + ">\n";

            return result;
        }
    }
}

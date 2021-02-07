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
        //public int Level { get; set; }
        private int _level;
                                                                                    
        /* Constructors */
        public ASTNode(string tag, Token token = null, int level = 0)
        {
            Children = new List<ASTNode>();
            Tag = tag;
            Token = token;
            _level = level;
        }

        /* Public Methods */
        /// <summary>
        /// Adds a child node to the current node
        /// </summary>
        /// <param name="node">Node to be added</param>
        public void Add(ASTNode node)
        {
            //node.Level = Level + 1;
            Children.Add(node);
        }

        public static string Print(ASTNode node, int level, bool print)
        {
            string tabs = new string('\t', level);

            string result = "";

            if (print || node.Token != null)

            {
                result += tabs + "<" + node.Tag + ">\n";

                if (node.Token != null)
                {
                    result += tabs + '\t' + node.Token.ToString() + '\n';
                }

            }

            foreach (ASTNode child in node.Children)
            {
                if (child.Children.Count > 1 || child.Token != null)
                {
                    result += ASTNode.Print(child, level + 1, true);
                }
                else
                {
                    result += ASTNode.Print(child, level, false);
                }
            }

            if (print || node.Token != null)
            {
                result += tabs + "</" + node.Tag + ">\n";
            }
                return result;
            
        }
    }
}

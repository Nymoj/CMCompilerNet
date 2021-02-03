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
        public int Level { get; set; }


        /* Constructors */
        public ASTNode(string tag, Token token = null)
        {
            Children = new List<ASTNode>();
            Tag = tag;
            Token = token;
            Level = 0;
        }

        /* Public Methods */
        /// <summary>
        /// Adds a child node to the current node
        /// </summary>
        /// <param name="node">Node to be added</param>
        public void Add(ASTNode node)
        {
            node.Level = Level + 1;
            Children.Add(node);
        }

        /// <summary>
        /// A string representation of the subtree who's root is the current node
        /// </summary>
        /// <returns>A string representing the subtree who's root is the current node</returns>
        public override string ToString()
        {
            string tabs = new string('\t', Level);

            string result = "";
            result += tabs + Token?.ToString();

            foreach (ASTNode child in Children)
            {
                result += child.ToString();
            }

            /*return tabs + "<" + Tag + ">" + '\n' +
                   "\t" + tabs + result +
                   tabs + "</" + Tag + ">" + '\n';*/
            //return string.Format("{0}<{1}>\n{0}\t{2}\n{0}</{1}>\n", tabs, Tag, result);
            return tabs + "<" + Tag + ">" + '\n' +
                   "\t" + tabs + result + '\n' +
                   tabs + "</" + Tag + ">\n";
        }
    }
}

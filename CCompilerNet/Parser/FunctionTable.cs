using CCompilerNet.CodeGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CCompilerNet.Parser
{
    public class FunctionTable
    {
        // ID : FunctionSymbol
        private Dictionary<string, FunctionSymbol> _fs;
        private TypeBuilder _typeBuilder;

        public FunctionTable(TypeBuilder typeBuilder)
        {
            _fs = new Dictionary<string, FunctionSymbol>();
            _typeBuilder = typeBuilder;
        }

        /*public void Define(string name, List<string> parmTypeList, string type)
        {
            _fs.Add(name, new FunctionSymbol(parmTypeList,
                type,
                _typeBuilder.DefineMethod(name,
                System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
                VMWriter.ConvertToType(type, false),
                VMWriter.ConvertToType(parmTypeList.ToArray())
                )
            ));
        }*/

        public void Define(ASTNode root)
        {
            string type = SemanticHelper.GetFunctionType(root);
            string id = SemanticHelper.GetFunctionId(root);
            List<string> parmTypeList = SemanticHelper.GetFunctionParmTypes(root);

            _fs.Add(id, new FunctionSymbol(
                    parmTypeList,
                    type, 
                    _typeBuilder.DefineMethod(
                        id, 
                        System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
                        VMWriter.ConvertToType(type, false),
                        VMWriter.ConvertToType(parmTypeList?.ToArray())
                    )
                )
            );
        }

        public FunctionSymbol GetFunctionSymbol(string name)
        {
            if (!FunctionSymbolExists(name))
            {
                return null;
            }

            return _fs[name];
        }

        public bool FunctionSymbolExists(string name)
        {
            return _fs.ContainsKey(name);
        }
    }
}

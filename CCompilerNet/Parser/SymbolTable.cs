﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CCompilerNet.Parser
{
    public enum Kind
    {
        STATIC,
        LOCAL,
        ARG,
        THAT
    }

    public class SymbolTable
    {
        private Dictionary<string, Symbol> _st;
        /*public SymbolTable Scope { get; private set; }
        private SymbolTable _next;*/

        private SymbolTable _head;
        private SymbolTable _next;

        private int _staticIndex;
        private int _localIndex;
        private int _argIndex;
        private int _thatIndex;

        public SymbolTable(SymbolTable next)
        {
            _st = new Dictionary<string, Symbol>();

            _next = next;
            _head = null;

            _staticIndex = 0;
            _localIndex = 0;
            _argIndex = 0;
            _thatIndex = 0;
        }

        public void Reset()
        {
            _st = new Dictionary<string, Symbol>();
            _staticIndex = 0;
            _localIndex = 0;
            _argIndex = 0;
            _thatIndex = 0;
            _head = null;
        }

        public SymbolTable StartSubRoutine()
        {
            _head = new SymbolTable(this);
            return _head;
        }

        public SymbolTable GetNext()
        {
            return _next;
        }

        public void Define(string name, string type, Kind kind)
        {
            Symbol symbol = new Symbol(type, kind);

            if (SymbolExists(name))
            {
                Console.WriteLine($"Error: {name} already exists in the current scope");
                return;
            }

            switch(kind)
            {
                case Kind.STATIC:
                    symbol.Index = _staticIndex;
                    _staticIndex++;
                    break;
                case Kind.LOCAL:
                    symbol.Index = _localIndex;
                    _localIndex++;
                    break;
                case Kind.ARG:
                    symbol.Index = _argIndex;
                    _argIndex++;
                    break;
                case Kind.THAT:
                    symbol.Index = _thatIndex;
                    _thatIndex++;
                    break;
            }

            _st.Add(name, symbol);
        }

        public Symbol GetSymbol(string name)
        {
            /*if (_next == null || !_next.SymbolExists(name))
            {
                return null;
            }
            if (!_st.ContainsKey(name))
            {
                return _next.GetSymbol(name);
            }

            return _next.GetSymbol(name);*/

            if (_next == null)
            {
                return SymbolExists(name) ? _st[name] : null;
            }

            if (SymbolExists(name))
            {
                return _st[name];
            }

            return _next.GetSymbol(name);
        }

        public void Define(string name, string type, Kind kind, LocalBuilder localBuilder)
        {
            Symbol symbol = new Symbol(type, kind);

            if (SymbolExists(name))
            {
                Console.WriteLine($"Error: {name} already exists in the current scope");
                return;
            }

            switch (kind)
            {
                case Kind.STATIC:
                    symbol.Index = _staticIndex;
                    _staticIndex++;
                    break;
                case Kind.LOCAL:
                    symbol.Index = _localIndex;
                    localBuilder.SetLocalSymInfo(name);
                    symbol.LocalBuilder = localBuilder;
                    _localIndex++;
                    break;
                case Kind.ARG:
                    symbol.Index = _argIndex;
                    _argIndex++;
                    break;
                case Kind.THAT:
                    symbol.Index = _thatIndex;
                    _thatIndex++;
                    break;
            }

            _st.Add(name, symbol);
        }

        public void Define(string name, string type, Kind kind, int arrayLength, LocalBuilder localBuilder)
        {
            Symbol symbol = new Symbol(type, kind, arrayLength);

            if (SymbolExists(name))
            {
                Console.WriteLine($"Error: {name} already exists in the current scope");
                return;
            }

            switch (kind)
            {
                case Kind.STATIC:
                    symbol.Index = _staticIndex;
                    _staticIndex++;
                    break;
                case Kind.LOCAL:
                    symbol.Index = _localIndex;
                    localBuilder.SetLocalSymInfo(name);
                    symbol.LocalBuilder = localBuilder;
                    _localIndex++;
                    break;
                case Kind.ARG:
                    symbol.Index = _argIndex;
                    _argIndex++;
                    break;
                case Kind.THAT:
                    symbol.Index = _thatIndex;
                    _thatIndex++;
                    break;
            }

            _st.Add(name, symbol);
        }

        public bool SymbolExists(string name)
        {
            return _st.ContainsKey(name);
        }

        public int VarCount(Kind kind)
        {
            switch(kind)
            {
                case Kind.STATIC:
                    return _staticIndex;
                case Kind.LOCAL:
                    return _localIndex;
                case Kind.ARG:
                    return _argIndex;
                case Kind.THAT:
                    return _thatIndex;
            }

            // error
            return -1;
        }
    }
}

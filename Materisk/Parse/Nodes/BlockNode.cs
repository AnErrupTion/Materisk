﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class BlockNode : SyntaxNode
{
    private readonly List<SyntaxNode> _nodes;

    public BlockNode(List<SyntaxNode> nodes)
    {
        _nodes = nodes;
    }

    public override NodeType Type => NodeType.Block;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        object lastVal = null;

        foreach (var node in _nodes)
        {
            var res = node.Emit(variables, module, type, method, arguments);

            if (res != null)
                lastVal = res;
        }

        return lastVal;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (var node in _nodes) yield return node;
    }

    public override string ToString()
    {
        return "BlockNode:";
    }
}
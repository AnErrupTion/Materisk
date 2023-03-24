﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class IfNode : SyntaxNode
{
    public List<(SyntaxNode cond, SyntaxNode block)> Conditions { get; } = new();

    public override NodeType Type => NodeType.If;

    public override SValue Evaluate(Scope scope)
    {
        foreach ((var cond, var block) in Conditions)
        {
            var condRes = cond.Evaluate(scope);

            if (condRes.IsTruthy())
            {
                return block.Evaluate(new Scope(scope));
            }
        }

        return SValue.Null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (var (cond, block) in Conditions)
        {
            yield return cond;
            yield return block;
        }
    }

    internal void AddCase(SyntaxNode cond, SyntaxNode block)
    {
        Conditions.Add((cond, block));
    }

    public override string ToString()
    {
        return "IfNode:";
    }
}
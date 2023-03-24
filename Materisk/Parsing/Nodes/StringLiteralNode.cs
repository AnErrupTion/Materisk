﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class StringLiteralNode : SyntaxNode
{
    private readonly SyntaxToken syntaxToken;

    public StringLiteralNode(SyntaxToken syntaxToken)
    {
        this.syntaxToken = syntaxToken;
    }

    public override NodeType Type => NodeType.StringLiteral;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var value = syntaxToken.Value.ToString();
        method.CilMethodBody?.Instructions.Add(CilOpCodes.Ldstr, value);
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(syntaxToken);
    }

    public override string ToString()
    {
        return "StringLitNode:";
    }
}
﻿using System.Globalization;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class FloatLiteralNode : SyntaxNode
{
    private readonly SyntaxToken _syntaxToken;

    public FloatLiteralNode(SyntaxToken syntaxToken)
    {
        _syntaxToken = syntaxToken;
    }

    public override NodeType Type => NodeType.FloatLiteral;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var value = float.Parse(_syntaxToken.Text, CultureInfo.InvariantCulture);
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldc_R4, value);
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_syntaxToken);
    }

    public override string ToString()
    {
        return "FloatLitNode:";
    }
}
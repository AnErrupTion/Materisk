﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class FieldDefinitionNode : SyntaxNode
{
    private readonly bool isPublic;
    private readonly SyntaxToken nameToken;
    private readonly SyntaxToken type;
    private readonly SyntaxNode? statement;

    public FieldDefinitionNode(bool isPublic, SyntaxToken nameToken, SyntaxToken type, SyntaxNode? statement = null)
    {
        this.isPublic = isPublic;
        this.nameToken = nameToken;
        this.type = type;
        this.statement = statement;
    }

    public override NodeType Type => NodeType.FieldDefinition;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var attributes = FieldAttributes.Static;

        if (isPublic)
            attributes |= FieldAttributes.Public;

        var newField = new FieldDefinition(nameToken.Text,
            attributes,
            Utils.GetTypeSignatureFor(module, type.Text));

        if (statement != null)
        {
            statement.Emit(variables, module, method, arguments);
            method.CilMethodBody?.Instructions.Add(CilOpCodes.Stsfld, newField);
        }

        module.TopLevelTypes[1].Fields.Add(newField);

        return newField;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(nameToken);
        yield return statement;
    }
}
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class ModuleFieldDefinitionNode : SyntaxNode
{
    private readonly bool isPublic;
    private readonly bool isStatic;
    private readonly SyntaxToken nameToken;
    private readonly SyntaxToken typeToken;

    public ModuleFieldDefinitionNode(bool isPublic, bool isStatic, SyntaxToken nameToken, SyntaxToken typeToken)
    {
        this.isPublic = isPublic;
        this.isStatic = isStatic;
        this.nameToken = nameToken;
        this.typeToken = typeToken;
    }

    public override NodeType Type => NodeType.ModuleFieldDefinition;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        FieldAttributes attributes = 0;

        if (isPublic)
            attributes |= FieldAttributes.Public;

        if (isStatic)
            attributes |= FieldAttributes.Static;

        var newField = new FieldDefinition(nameToken.Text,
            attributes,
            Utils.GetTypeSignatureFor(module, typeToken.Text));

        type.Fields.Add(newField);

        return newField;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(nameToken);
        yield return new TokenNode(typeToken);
    }
}
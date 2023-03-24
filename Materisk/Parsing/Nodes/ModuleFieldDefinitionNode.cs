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
    private readonly SyntaxToken type;

    public ModuleFieldDefinitionNode(bool isPublic, bool isStatic, SyntaxToken nameToken, SyntaxToken type)
    {
        this.isPublic = isPublic;
        this.isStatic = isStatic;
        this.nameToken = nameToken;
        this.type = type;
    }

    public override NodeType Type => NodeType.ModuleFieldDefinition;

    public override SValue Evaluate(Scope scope)
    {
        throw new NotImplementedException();
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        FieldAttributes attributes = 0;

        if (isPublic)
            attributes |= FieldAttributes.Public;

        if (isStatic)
            attributes |= FieldAttributes.Static;

        var newField = new FieldDefinition(nameToken.Text,
            attributes,
            Utils.GetTypeSignatureFor(module, type.Text));

        return newField;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(nameToken);
        yield return new TokenNode(type);
    }
}
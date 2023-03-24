using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class ModuleFieldDefinitionNode : SyntaxNode
{
    private readonly bool isPublic;
    private readonly bool isStatic;
    private readonly SyntaxToken nameToken;
    private readonly SyntaxToken type;
    private readonly SyntaxNode? statement;

    public ModuleFieldDefinitionNode(bool isPublic, bool isStatic, SyntaxToken nameToken, SyntaxToken type, SyntaxNode? statement = null)
    {
        this.isPublic = isPublic;
        this.isStatic = isStatic;
        this.nameToken = nameToken;
        this.type = type;
        this.statement = statement;
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

        if (statement != null)
        {
            statement.Emit(variables, module, method, arguments);
            method.CilMethodBody?.Instructions.Add(isStatic ? CilOpCodes.Stsfld : CilOpCodes.Stfld, newField);
        }

        return newField;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(nameToken);
        if (statement != null) yield return statement;
    }
}
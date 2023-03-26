using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class FieldDefinitionNode : SyntaxNode
{
    private readonly bool _isPublic;
    private readonly SyntaxToken _nameToken;
    private readonly SyntaxToken _typeToken;
    private readonly SyntaxNode? _statement;

    public FieldDefinitionNode(bool isPublic, SyntaxToken nameToken, SyntaxToken typeToken, SyntaxNode? statement = null)
    {
        _isPublic = isPublic;
        _nameToken = nameToken;
        _typeToken = typeToken;
        _statement = statement;
    }

    public override NodeType Type => NodeType.FieldDefinition;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var attributes = FieldAttributes.Static;

        if (_isPublic)
            attributes |= FieldAttributes.Public;

        var newField = new FieldDefinition(_nameToken.Text,
            attributes,
            Utils.GetTypeSignatureFor(module, _typeToken.Text));

        if (_statement != null)
        {
            _statement.Emit(variables, module, type, method, arguments);
            method.CilMethodBody!.Instructions.Add(CilOpCodes.Stsfld, newField);
        }

        module.TopLevelTypes[1].Fields.Add(newField);

        return newField;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_nameToken);
        yield return _statement;
    }
}
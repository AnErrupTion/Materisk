using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class FieldDefinitionNode : SyntaxNode
{
    private readonly bool _isPublic;
    private readonly SyntaxToken _nameToken;
    private readonly SyntaxToken _typeToken;
    private readonly SyntaxToken? _secondTypeToken;

    public FieldDefinitionNode(bool isPublic, SyntaxToken nameToken, SyntaxToken typeToken, SyntaxToken? secondTypeToken)
    {
        _isPublic = isPublic;
        _nameToken = nameToken;
        _typeToken = typeToken;
        _secondTypeToken = secondTypeToken;
    }

    public override NodeType Type => NodeType.FieldDefinition;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    // TODO: Non-static fields
    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var pointerElementType = _typeToken.Text switch
        {
            "ptr" or "arr" when _secondTypeToken is not null => TypeSigUtils.GetTypeSignatureFor(_secondTypeToken.Text),
            "str" => LLVMTypeRef.Int8,
            _ => null
        };

        var newField = new MateriskField(module.Types[0], _nameToken.Text,
            TypeSigUtils.GetTypeSignatureFor(_typeToken.Text), pointerElementType, _typeToken.Text[0] is 'i');
        module.Types[0].Fields.Add(newField);

        return newField;
    }
}
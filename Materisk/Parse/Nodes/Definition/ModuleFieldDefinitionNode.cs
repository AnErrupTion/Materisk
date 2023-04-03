using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class ModuleFieldDefinitionNode : SyntaxNode
{
    private readonly bool _isPublic;
    private readonly bool _isStatic;
    private readonly SyntaxToken _nameToken;
    private readonly SyntaxToken _typeToken;
    private readonly SyntaxToken? _secondTypeToken;

    public ModuleFieldDefinitionNode(bool isPublic, bool isStatic, SyntaxToken nameToken, SyntaxToken typeToken, SyntaxToken? secondTypeToken)
    {
        _isPublic = isPublic;
        _isStatic = isStatic;
        _nameToken = nameToken;
        _typeToken = typeToken;
        _secondTypeToken = secondTypeToken;
    }

    public override NodeType Type => NodeType.ModuleFieldDefinition;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var pointerElementType = _typeToken.Text switch
        {
            "ptr" or "arr" when _secondTypeToken is not null => TypeSigUtils.GetTypeSignatureFor(_secondTypeToken.Text),
            "str" => LLVMTypeRef.Int8,
            _ => null
        };

        var newField = new MateriskField(type, _nameToken.Text,
            TypeSigUtils.GetTypeSignatureFor(_typeToken.Text), pointerElementType, _typeToken.Text[0] is 'i');
        type.Fields.Add(newField);

        return newField;
    }
}
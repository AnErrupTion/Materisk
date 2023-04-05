using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class FieldDefinitionNode : SyntaxNode
{
    private readonly bool _isPublic;
    private readonly string _name;
    private readonly string _type;
    private readonly string _secondType;

    public FieldDefinitionNode(bool isPublic, string nameToken, string typeToken, string secondTypeToken)
    {
        _isPublic = isPublic;
        _name = nameToken;
        _type = typeToken;
        _secondType = secondTypeToken;
    }

    public override NodeType Type => NodeType.FieldDefinition;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var pointerElementType = _type switch
        {
            "ptr" or "arr" when !string.IsNullOrEmpty(_secondType) => TypeSigUtils.GetTypeSignatureFor(module, _secondType),
            "str" => LLVMTypeRef.Int8,
            _ => null
        };

        var newField = new MateriskField(module.Types[0], _name,
            MateriskAttributesUtils.CreateAttributes(_isPublic, true, false),
            TypeSigUtils.GetTypeSignatureFor(module, _type), pointerElementType, _type[0] is 'i');
        module.Types[0].Fields.Add(newField);

        return newField;
    }
}
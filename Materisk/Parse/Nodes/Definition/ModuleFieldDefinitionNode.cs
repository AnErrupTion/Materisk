using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class ModuleFieldDefinitionNode : SyntaxNode
{
    private readonly bool _isPublic;
    private readonly bool _isStatic;
    private readonly string _name;
    private readonly string _type;
    private readonly string _secondType;

    public ModuleFieldDefinitionNode(bool isPublic, bool isStatic, string name, string type, string secondType)
    {
        _isPublic = isPublic;
        _isStatic = isStatic;
        _name = name;
        _type = type;
        _secondType = secondType;
    }

    public override NodeType Type => NodeType.ModuleFieldDefinition;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var pointerElementType = _type switch
        {
            "ptr" or "arr" when !string.IsNullOrEmpty(_secondType) => TypeSigUtils.GetTypeSignatureFor(module, _secondType),
            "str" => LLVMTypeRef.Int8,
            _ => null
        };

        var newField = new MateriskField(type, _name,
            MateriskAttributesUtils.CreateAttributes(_isPublic, _isStatic, false, false),
            TypeSigUtils.GetTypeSignatureFor(module, _type), pointerElementType, _type[0] is 'i');

        type.Fields.Add(newField);

        return newField;
    }
}
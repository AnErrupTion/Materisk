using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class FieldDefinitionNode : SyntaxNode
{
    private readonly bool _isPublic;
    private readonly bool _isStatic;
    private readonly string _name;
    private readonly string _type;
    private readonly string _secondType;

    public FieldDefinitionNode(bool isPublic, bool isStatic, string name, string type, string secondType)
    {
        _isPublic = isPublic;
        _isStatic = isStatic;
        _name = name;
        _type = type;
        _secondType = secondType;
    }

    public override NodeType Type => NodeType.FieldDefinition;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        string typeName;
        LLVMTypeRef pointerElementType = null;

        switch (_type)
        {
            case "ptr" or "arr" when !string.IsNullOrEmpty(_secondType):
            {
                typeName = _secondType;
                pointerElementType = TypeSigUtils.GetTypeSignatureFor(module, _secondType, _type is "ptr");
                break;
            }
            case "str":
            {
                typeName = "str";
                pointerElementType = LLVMTypeRef.Int8;
                break;
            }
            default:
            {
                typeName = _type;
                break;
            }
        }

        var newField = new MateriskField(type, _name,
            MateriskAttributesUtils.CreateAttributes(_isPublic, _isStatic, false, false, false), typeName,
            TypeSigUtils.GetTypeSignatureFor(module, _type, _type is "ptr"), pointerElementType, _type[0] is 'i');

        type.Fields.Add(newField);

        return newField;
    }
}
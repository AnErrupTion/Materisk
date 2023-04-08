using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Identifier;

internal class InitVariableNode : SyntaxNode
{
    private readonly bool _mutable;
    private readonly string _name;
    private readonly string _type;
    private readonly string _secondType;
    private readonly SyntaxNode _expr;

    public InitVariableNode(bool mutable, string name, string type, string secondType, SyntaxNode expr)
    {
        _mutable = mutable;
        _name = name;
        _type = type;
        _secondType = secondType;
        _expr = expr;
    }

    public override NodeType Type => NodeType.InitVariable;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        if (method.GetVariableByName(_name) is not null)
            throw new InvalidOperationException($"Can not initialize the same variable twice: {module.Name}.{type.Name}.{method.Name}.{_name}");

        var value = _expr.Emit(module, type, method, thenBlock, elseBlock).Load();
        var valueType = value.TypeOf;

        string typeName;
        LLVMTypeRef pointerElementType = null;

        switch (_type)
        {
            case "ptr" or "arr" when !string.IsNullOrEmpty(_secondType):
            {
                value = module.LlvmBuilder.BuildIntToPtr(value, LLVMTypeRef.CreatePointer(valueType, 0));
                typeName = _secondType;
                pointerElementType = TypeSigUtils.GetTypeSignatureFor(module, _secondType);
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

        if (_mutable)
        {
            var constValue = value;
            value = module.LlvmBuilder.BuildAlloca(value.TypeOf);
            module.LlvmBuilder.BuildStore(constValue, value);
        }

        var variable = new MateriskLocalVariable(method, _name, _mutable, typeName, valueType, pointerElementType, _type[0] is 'i', value);
        method.Variables.Add(variable);
        return variable;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _expr;
    }
}
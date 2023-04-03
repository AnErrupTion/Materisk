using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
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

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        // TODO: Check in parser
        if (string.IsNullOrEmpty(_name))
            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

        if (method.GetVariableByName(_name) is not null)
            throw new InvalidOperationException("Can not initialize the same variable twice!");

        var value = (LLVMValueRef)_expr.Emit(module, type, method, metadata);
        var valueType = value.TypeOf;

        LLVMTypeRef pointerElementType = null;

        switch (_type)
        {
            case "ptr" or "arr" when !string.IsNullOrEmpty(_secondType):
            {
                value = module.LlvmBuilder.BuildIntToPtr(value, LLVMTypeRef.CreatePointer(valueType, 0));
                pointerElementType = TypeSigUtils.GetTypeSignatureFor(_secondType);
                break;
            }
            case "str":
            {
                pointerElementType = LLVMTypeRef.Int8;
                break;
            }
        }

        if (_mutable)
        {
            var constValue = value;
            value = module.LlvmBuilder.BuildAlloca(value.TypeOf);
            module.LlvmBuilder.BuildStore(constValue, value);
        }

        method.Variables.Add(new(method, _name, _mutable, valueType, pointerElementType, _type[0] is 'i', value));
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _expr;
    }
}
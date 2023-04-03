using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Identifier;

internal class InitVariableNode : SyntaxNode
{
    private readonly bool _mutable;
    private readonly SyntaxToken _identToken;
    private readonly SyntaxToken _typeToken;
    private readonly SyntaxToken? _secondTypeToken;
    private readonly SyntaxNode _expr;

    public InitVariableNode(bool mutable, SyntaxToken identToken, SyntaxToken typeToken, SyntaxToken? secondTypeToken, SyntaxNode expr)
    {
        _mutable = mutable;
        _identToken = identToken;
        _typeToken = typeToken;
        _secondTypeToken = secondTypeToken;
        _expr = expr;
    }

    public override NodeType Type => NodeType.InitVariable;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var name = _identToken.Text;

        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

        if (method.GetVariableByName(name) is not null)
            throw new InvalidOperationException("Can not initialize the same variable twice!");

        var value = (LLVMValueRef)_expr.Emit(module, type, method, metadata);
        var valueType = value.TypeOf;

        LLVMTypeRef pointerElementType = null;

        switch (_typeToken.Text)
        {
            case "ptr" or "arr" when _secondTypeToken is not null:
            {
                value = module.LlvmBuilder.BuildIntToPtr(value, LLVMTypeRef.CreatePointer(valueType, 0));
                pointerElementType = TypeSigUtils.GetTypeSignatureFor(_secondTypeToken.Text);
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
        method.Variables.Add(new(method, name, _mutable, valueType, pointerElementType, _typeToken.Text[0] is 'i', value));
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_identToken);
        yield return new TokenNode(_typeToken);
        if (_secondTypeToken is not null) yield return new TokenNode(_secondTypeToken);
        yield return _expr;
    }

    public override string ToString()
    {
        return "InitVariableNode:";
    }
}
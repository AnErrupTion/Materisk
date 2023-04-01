using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;

namespace Materisk.Parse.Nodes.Operator;

internal class UnaryExpressionNode : SyntaxNode
{
    private readonly SyntaxToken _token;
    private readonly SyntaxNode _rhs;

    public UnaryExpressionNode(SyntaxToken token, SyntaxNode rhs)
    {
        _token = token;
        _rhs = rhs;
    }

    public override NodeType Type => NodeType.UnaryExpression;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        _rhs.Emit(variables, module, type, method, arguments);

        switch (_token.Type)
        {
            case SyntaxType.Bang:
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldc_I4_0);
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Ceq);
                break;
            case SyntaxType.Minus:
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Neg);
                break;
            case SyntaxType.Plus:
                break;
            default:
                throw new InvalidOperationException($"Trying to do a unary expression on: {_token.Type}");
        }

        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        var value = (LLVMValueRef)_rhs.Emit(module, type, method);

        LLVMValueRef resultValue;

        switch (_token.Type)
        {
            // TODO: Float compare and negation
            case SyntaxType.Bang: resultValue = module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, value, LlvmUtils.IntZero); break;
            case SyntaxType.Minus: resultValue = module.LlvmBuilder.BuildNeg(value); break;
            case SyntaxType.Plus: return value;
            default: throw new InvalidOperationException($"Trying to do a unary expression on: {_token.Type}");
        }

        return resultValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_token);
        yield return _rhs;
    }

    public override string ToString()
    {
        return "UnaryExpressionNode:";
    }
}
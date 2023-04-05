using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Operator;

internal class UnaryExpressionNode : SyntaxNode
{
    private readonly string _operator;
    private readonly SyntaxNode _rhs;

    public UnaryExpressionNode(string op, SyntaxNode rhs)
    {
        _operator = op;
        _rhs = rhs;
    }

    public override NodeType Type => NodeType.UnaryExpression;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = (LLVMValueRef)_rhs.Emit(module, type, method, metadata);
        var resultValue = _operator switch
        {
            "!" => value.TypeOf == LLVMTypeRef.Float || value.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, value, LlvmUtils.IntZero)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, value, LlvmUtils.IntZero),
            "-" => value.TypeOf == LLVMTypeRef.Float || value.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFNeg(value)
                : module.LlvmBuilder.BuildNeg(value),
            "+" => value,
            _ => throw new InvalidOperationException($"Trying to do a unary expression on: \"{_operator}\"")
        };
        return resultValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _rhs;
    }
}
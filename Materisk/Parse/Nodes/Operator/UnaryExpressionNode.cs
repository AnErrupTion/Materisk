using LLVMSharp.Interop;
using Materisk.TypeSystem;

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

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var value = _rhs.Emit(module, type, method, thenBlock, elseBlock).Load();
        var resultValue = _operator switch
        {
            "!" => value.TypeOf == LLVMTypeRef.Int1
                ? module.LlvmBuilder.BuildNot(value)
                : throw new InvalidOperationException($"Trying to do a unary bang expression on: {value.TypeOf}"),
            "-" => value.TypeOf == LLVMTypeRef.Float || value.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFNeg(value)
                : module.LlvmBuilder.BuildNeg(value)
        };
        return resultValue.ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _rhs;
    }
}
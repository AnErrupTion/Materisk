using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Operator;

internal class BinaryExpressionNode : SyntaxNode
{
    private readonly SyntaxNode _left;
    private readonly string _operator;
    private readonly SyntaxNode _right;

    public BinaryExpressionNode(SyntaxNode left, string op, SyntaxNode right)
    {
        _left = left;
        _operator = op;
        _right = right;
    }

    public override NodeType Type => NodeType.BinaryExpression;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var lhs = _left.Emit(module, type, method, metadata);
        var rhs = _right.Emit(module, type, method, metadata);

        LLVMValueRef leftValue, rightValue;
        bool leftSigned, rightSigned;

        if (lhs is MateriskUnit leftUnit)
        {
            leftValue = leftUnit.Load();
            leftSigned = leftUnit.Signed;
        }
        else
        {
            leftValue = (LLVMValueRef)lhs;
            leftSigned = false; // We can't guess!
        }

        if (rhs is MateriskUnit rightUnit)
        {
            rightValue = rightUnit.Load();
            rightSigned = rightUnit.Signed;
        }
        else
        {
            rightValue = (LLVMValueRef)rhs;
            rightSigned = false; // We can't guess!
        }

        if (leftSigned && !rightSigned || !leftSigned && rightSigned)
            throw new InvalidOperationException("Both operands need to be either signed or unsigned!");

        return EmitOperation(module, leftValue, rightValue, leftSigned);
    }

    private LLVMValueRef EmitOperation(MateriskModule module, LLVMValueRef leftValue, LLVMValueRef rightValue, bool leftSigned)
    {
        var resultValue = _operator switch
        {
            "+=" or "++" or "+" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFAdd(leftValue, rightValue)
                : module.LlvmBuilder.BuildAdd(leftValue, rightValue),
            "-=" or "--" or "-" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFSub(leftValue, rightValue)
                : module.LlvmBuilder.BuildSub(leftValue, rightValue),
            "/=" or "/" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFDiv(leftValue, rightValue)
                : leftSigned
                    ? module.LlvmBuilder.BuildSDiv(leftValue, rightValue)
                    : module.LlvmBuilder.BuildUDiv(leftValue, rightValue),
           "*=" or "*" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFMul(leftValue, rightValue)
                : module.LlvmBuilder.BuildMul(leftValue, rightValue),
            "%=" or "%" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFRem(leftValue, rightValue)
                : leftSigned
                    ? module.LlvmBuilder.BuildSRem(leftValue, rightValue)
                    : module.LlvmBuilder.BuildURem(leftValue, rightValue),
            "!=" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntNE, leftValue, rightValue),
            "==" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, leftValue, rightValue),
            "<" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntULT, leftValue, rightValue),
            "<=" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ,
                    module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, leftValue, rightValue),
                    LlvmUtils.IntZero)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ,
                    module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, leftValue, rightValue),
                    LlvmUtils.IntZero),
            ">" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, leftValue, rightValue),
            ">=" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ,
                    module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, leftValue, rightValue),
                    LlvmUtils.IntZero)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ,
                    module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntULT, leftValue, rightValue),
                    LlvmUtils.IntZero),
            _ => throw new InvalidOperationException($"Trying to do a binary expression on: \"{_operator}\"")
        };

        return resultValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _left;
        yield return _right;
    }
}
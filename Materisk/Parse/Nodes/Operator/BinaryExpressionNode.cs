﻿using LLVMSharp.Interop;
using Materisk.TypeSystem;

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

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var lhs = _left.Emit(module, type, method, thenBlock, elseBlock);
        var rhs = _right.Emit(module, type, method, thenBlock, elseBlock);

        if (lhs.Signed && !rhs.Signed || !lhs.Signed && rhs.Signed)
            throw new InvalidOperationException("Both operands need to be either signed or unsigned!");

        return EmitOperation(module, lhs.Load(), rhs.Load(), lhs.Signed);
    }

    private MateriskValue EmitOperation(MateriskModule module, LLVMValueRef leftValue, LLVMValueRef rightValue, bool leftSigned)
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
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntULE, leftValue, rightValue),
            ">" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, leftValue, rightValue),
            ">=" => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntUGE, leftValue, rightValue),
            "&&" => module.LlvmBuilder.BuildAnd(leftValue, rightValue),
            "||" => module.LlvmBuilder.BuildOr(leftValue, rightValue)
        };

        return resultValue.ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _left;
        yield return _right;
    }
}
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;

namespace Materisk.Parse.Nodes.Operator;

internal class BinaryExpressionNode : SyntaxNode
{
    private readonly SyntaxNode _left;
    private readonly SyntaxToken _operatorToken;
    private readonly SyntaxNode _right;

    public BinaryExpressionNode(SyntaxNode left, SyntaxToken operatorToken, SyntaxNode right)
    {
        _left = left;
        _operatorToken = operatorToken;
        _right = right;
    }

    public override NodeType Type => NodeType.BinaryExpression;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

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

        return EmitOperation(module, leftValue, rightValue, leftSigned, rightSigned);
    }

    private LLVMValueRef EmitOperation(MateriskModule module, LLVMValueRef leftValue, LLVMValueRef rightValue, bool leftSigned, bool rightSigned)
    {
        var resultValue = _operatorToken.Type switch
        {
            SyntaxType.PlusEquals or SyntaxType.PlusPlus or SyntaxType.Plus => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFAdd(leftValue, rightValue)
                : module.LlvmBuilder.BuildAdd(leftValue, rightValue),
            SyntaxType.MinusEquals or SyntaxType.MinusMinus or SyntaxType.Minus => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFSub(leftValue, rightValue)
                : module.LlvmBuilder.BuildSub(leftValue, rightValue),
            SyntaxType.DivEquals or SyntaxType.Div => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFDiv(leftValue, rightValue)
                : leftSigned
                    ? module.LlvmBuilder.BuildSDiv(leftValue, rightValue)
                    : module.LlvmBuilder.BuildUDiv(leftValue, rightValue),
            SyntaxType.MulEquals or SyntaxType.Mul => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFMul(leftValue, rightValue)
                : module.LlvmBuilder.BuildMul(leftValue, rightValue),
            SyntaxType.ModEquals or SyntaxType.Mod => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFRem(leftValue, rightValue)
                : leftSigned
                    ? module.LlvmBuilder.BuildSRem(leftValue, rightValue)
                    : module.LlvmBuilder.BuildURem(leftValue, rightValue),
            SyntaxType.BangEquals => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntNE, leftValue, rightValue),
            SyntaxType.EqualsEquals => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, leftValue, rightValue),
            SyntaxType.LessThan => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntULT, leftValue, rightValue),
            SyntaxType.LessThanEqu => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ,
                    module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, leftValue, rightValue),
                    LlvmUtils.IntZero)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ,
                    module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, leftValue, rightValue),
                    LlvmUtils.IntZero),
            SyntaxType.GreaterThan => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, leftValue, rightValue)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, leftValue, rightValue),
            SyntaxType.GreaterThanEqu => leftValue.TypeOf == LLVMTypeRef.Float || leftValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ,
                    module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, leftValue, rightValue),
                    LlvmUtils.IntZero)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ,
                    module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntULT, leftValue, rightValue),
                    LlvmUtils.IntZero),
            _ => throw new InvalidOperationException($"Trying to do a binary expression on: {_operatorToken.Type}")
        };

        return resultValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _left;
        yield return new TokenNode(_operatorToken);
        yield return _right;
    }

    public override string ToString()
    {
        return "BinExprNode: op=" + _operatorToken.Type;
    }
}
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
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
        _left.Emit(variables, module, type, method, arguments);
        _right.Emit(variables, module, type, method, arguments);

        switch (_operatorToken.Type)
        { 
            case SyntaxType.PlusEquals:
            case SyntaxType.PlusPlus:
            case SyntaxType.Plus: method.CilMethodBody!.Instructions.Add(CilOpCodes.Add); break;
            case SyntaxType.MinusEquals:
            case SyntaxType.MinusMinus:
            case SyntaxType.Minus: method.CilMethodBody!.Instructions.Add(CilOpCodes.Sub); break;
            case SyntaxType.DivEquals:
            case SyntaxType.Div: method.CilMethodBody!.Instructions.Add(CilOpCodes.Div); break;
            case SyntaxType.MulEquals:
            case SyntaxType.Mul: method.CilMethodBody!.Instructions.Add(CilOpCodes.Mul); break;
            case SyntaxType.ModEquals:
            case SyntaxType.Mod: method.CilMethodBody!.Instructions.Add(CilOpCodes.Rem); break;
            case SyntaxType.EqualsEquals: method.CilMethodBody!.Instructions.Add(CilOpCodes.Ceq); break;
            case SyntaxType.LessThan: method.CilMethodBody!.Instructions.Add(CilOpCodes.Clt); break;
            case SyntaxType.LessThanEqu:
            {
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Cgt);
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldc_I4_0);
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Ceq);
                break;
            }
            case SyntaxType.GreaterThan: method.CilMethodBody!.Instructions.Add(CilOpCodes.Cgt); break;
            case SyntaxType.GreaterThanEqu:
            {
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Clt);
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldc_I4_0);
                method.CilMethodBody!.Instructions.Add(CilOpCodes.Ceq);
                break;
            }
            default: throw new InvalidOperationException($"Trying to do a binary expression on: {_operatorToken.Type}");
        }

        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        var leftValue = (LLVMValueRef)_left.Emit(module, type, method);
        var rightValue = (LLVMValueRef)_right.Emit(module, type, method);

        LLVMValueRef resultValue;

        // TODO: Unsigned and float
        switch (_operatorToken.Type)
        { 
            case SyntaxType.PlusEquals:
            case SyntaxType.PlusPlus:
            case SyntaxType.Plus: resultValue = module.LlvmBuilder.BuildAdd(leftValue, rightValue); break;
            case SyntaxType.MinusEquals:
            case SyntaxType.MinusMinus:
            case SyntaxType.Minus: resultValue = module.LlvmBuilder.BuildSub(leftValue, rightValue); break;
            case SyntaxType.DivEquals:
            case SyntaxType.Div: resultValue = module.LlvmBuilder.BuildSDiv(leftValue, rightValue); break;
            case SyntaxType.MulEquals:
            case SyntaxType.Mul: resultValue = module.LlvmBuilder.BuildMul(leftValue, rightValue); break;
            case SyntaxType.ModEquals:
            case SyntaxType.Mod: resultValue = module.LlvmBuilder.BuildSRem(leftValue, rightValue); break;
            case SyntaxType.EqualsEquals: resultValue = module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, leftValue, rightValue); break;
            case SyntaxType.LessThan: resultValue = module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, leftValue, rightValue); break;
            case SyntaxType.LessThanEqu:
            {
                var gtValue = module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, leftValue, rightValue);
                var zeroValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, true);
                resultValue = module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, gtValue, zeroValue);
                break;
            }
            case SyntaxType.GreaterThan: resultValue = module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, leftValue, rightValue); break;
            case SyntaxType.GreaterThanEqu:
            {
                var gtValue = module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, leftValue, rightValue);
                var zeroValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, true);
                resultValue = module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, gtValue, zeroValue);
                break;
            }
            default: throw new InvalidOperationException($"Trying to do a binary expression on: {_operatorToken.Type}");
        }

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
        return "BinaryExprNode: op=" + _operatorToken.Type;
    }
}
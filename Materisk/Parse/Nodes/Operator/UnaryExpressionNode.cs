﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;

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
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = (LLVMValueRef)_rhs.Emit(module, type, method, metadata);
        var resultValue = _token.Type switch
        {
            SyntaxType.Bang => value.TypeOf == LLVMTypeRef.Float || value.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, value, LlvmUtils.IntZero)
                : module.LlvmBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, value, LlvmUtils.IntZero),
            SyntaxType.Minus => value.TypeOf == LLVMTypeRef.Float || value.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFNeg(value)
                : module.LlvmBuilder.BuildNeg(value),
            SyntaxType.Plus => value,
            _ => throw new InvalidOperationException($"Trying to do a unary expression on: {_token.Type}")
        };
        return resultValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _rhs;
    }
}
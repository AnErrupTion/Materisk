﻿using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Literal;

internal class LongLiteralNode : SyntaxNode
{
    private readonly long _value;

    public LongLiteralNode(long value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.LongLiteral;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        if (_value < 0)
        {
            var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, Convert.ToUInt64(Math.Abs(_value)), true);
            return LLVMValueRef.CreateConstSub(LlvmUtils.LongZero, llvmValue).ToMateriskValue();
        }

        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, Convert.ToUInt64(_value), true).ToMateriskValue();
    }
}
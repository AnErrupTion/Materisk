﻿using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Literal;

internal class UIntLiteralNode : SyntaxNode
{
    private readonly uint _value;

    public UIntLiteralNode(uint value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.UIntLiteral;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(_value), true).ToMateriskValue();
    }
}
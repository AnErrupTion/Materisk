﻿using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Literal;

internal class UShortLiteralNode : SyntaxNode
{
    private readonly ushort _value;

    public UShortLiteralNode(ushort value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.UShortLiteral;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, Convert.ToUInt64(_value), true).ToMateriskValue();
    }
}
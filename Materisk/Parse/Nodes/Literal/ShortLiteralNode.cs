﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Literal;

internal class ShortLiteralNode : SyntaxNode
{
    private readonly short _value;

    public ShortLiteralNode(short value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.ShortLiteral;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        if (_value < 0)
        {
            var llvmValue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, Convert.ToUInt64(Math.Abs(_value)), true);
            return LLVMValueRef.CreateConstSub(LlvmUtils.ShortZero, llvmValue);
        }

        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, Convert.ToUInt64(_value), true);
    }
}
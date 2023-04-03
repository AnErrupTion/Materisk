﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Literal;

internal class ULongLiteralNode : SyntaxNode
{
    private readonly ulong _value;

    public ULongLiteralNode(ulong value)
    {
        _value = value;
    }

    public override NodeType Type => NodeType.ULongLiteral;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, Convert.ToUInt64(_value), true);
    }
}
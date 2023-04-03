﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;

namespace Materisk.Parse.Nodes.Misc;

internal class CastNode : SyntaxNode
{
    private readonly string _type;
    private readonly string _secondType;
    private readonly SyntaxNode _node;

    public CastNode(string type, string secondType, SyntaxNode node)
    {
        _type = type;
        _secondType = secondType;
        _node = node;
    }

    public override NodeType Type => NodeType.Cast;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    // TODO: Struct casts
    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = _node.Emit(module, type, method, metadata);
        var llvmValue = value is MateriskUnit unit ? unit.Load() : (LLVMValueRef)value;
        var resultValue = _type switch
        {
            "i8" or "u8" => module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Int8),
            "i16" or "u16" => module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Int16),
            "i32" or "u32" => module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Int32),
            "i64" or "u64" => module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Int64),
            "f32" => llvmValue.TypeOf == LLVMTypeRef.Float || llvmValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFPCast(llvmValue, LLVMTypeRef.Float)
                : module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Float),
            "f64" => llvmValue.TypeOf == LLVMTypeRef.Float || llvmValue.TypeOf == LLVMTypeRef.Double
                ? module.LlvmBuilder.BuildFPCast(llvmValue, LLVMTypeRef.Double)
                : module.LlvmBuilder.BuildIntCast(llvmValue, LLVMTypeRef.Double),
            "ptr" when !string.IsNullOrEmpty(_secondType) => _secondType switch
            {
                "i8" or "u8" => module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.BytePointer),
                "i16" or "u16" => module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.ShortPointer),
                "i32" or "u32" => module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.IntPointer),
                "i64" or "u64" => module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.LongPointer),
                "f32" => module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.FloatPointer),
                "f64" => module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.DoublePointer),
                "void" => module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.VoidPointer),
                _ => throw new InvalidOperationException($"Can not cast to pointer type: {_secondType}")
            },
            _ => throw new InvalidOperationException($"Can not cast to type: {_type}")
        };
        return resultValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _node;
    }
}
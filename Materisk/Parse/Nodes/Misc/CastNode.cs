using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

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

    // TODO: Struct casts
    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var llvmValue = _node.Emit(module, type, method, metadata).Load();
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
                _ => throw new InvalidOperationException($"Can not cast to pointer type \"{_secondType}\" in method: {module.Name}.{type.Name}.{method.Name}")
            },
            _ => throw new InvalidOperationException($"Can not cast to type \"{_type}\" in method: {module.Name}.{type.Name}.{method.Name}")
        };
        return resultValue.ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _node;
    }
}
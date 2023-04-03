using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;

namespace Materisk.Parse.Nodes.Misc;

internal class CastNode : SyntaxNode
{
    private readonly SyntaxToken _typeToken;
    private readonly SyntaxToken? _secondTypeToken;
    private readonly SyntaxNode _node;

    public CastNode(SyntaxToken typeToken, SyntaxToken? secondTypeToken, SyntaxNode node)
    {
        _typeToken = typeToken;
        _secondTypeToken = secondTypeToken;
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
        var resultValue = _typeToken.Text switch
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
            "ptr" when _secondTypeToken is not null => _secondTypeToken.Text switch
            {
                "i8" or "u8" => module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.BytePointer),
                "i16" or "u16" => module.LlvmBuilder.BuildIntCast(llvmValue, LlvmUtils.ShortPointer),
                "i32" or "u32" => module.LlvmBuilder.BuildIntCast(llvmValue, LlvmUtils.IntPointer),
                "i64" or "u64" => module.LlvmBuilder.BuildIntCast(llvmValue, LlvmUtils.LongPointer),
                "f32" => module.LlvmBuilder.BuildIntCast(llvmValue, LlvmUtils.FloatPointer),
                "f64" => module.LlvmBuilder.BuildIntCast(llvmValue, LlvmUtils.DoublePointer),
                "void" => module.LlvmBuilder.BuildPointerCast(llvmValue, LlvmUtils.VoidPointer),
                _ => throw new InvalidOperationException($"Can not cast to pointer type: {_secondTypeToken.Text}")
            },
            _ => throw new InvalidOperationException($"Can not cast to type: {_typeToken.Text}")
        };
        return resultValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_typeToken);
        if (_secondTypeToken is not null) yield return new TokenNode(_secondTypeToken);
        yield return _node;
    }
}
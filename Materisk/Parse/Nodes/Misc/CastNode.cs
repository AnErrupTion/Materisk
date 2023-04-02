using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;

namespace Materisk.Parse.Nodes.Misc;

internal class CastNode : SyntaxNode
{
    private readonly SyntaxToken _ident;
    private readonly SyntaxNode _node;

    public CastNode(SyntaxToken ident, SyntaxNode node)
    {
        _ident = ident;
        _node = node;
    }

    public override NodeType Type => NodeType.Cast;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    // TODO: Pointer and struct casts
    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = _node.Emit(module, type, method, metadata);
        var llvmValue = value is MateriskUnit unit ? unit.Load() : (LLVMValueRef)value;
        var resultValue = _ident.Text switch
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
            _ => throw new InvalidOperationException($"Can not cast to type: {_ident.Text}")
        };
        return resultValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_ident);
        yield return _node;
    }
}
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

    // TODO: Fix casts from float
    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = (LLVMValueRef)_node.Emit(module, type, method, metadata);
        var resultValue = _ident.Text switch
        {
            "i32" or "u32" => module.LlvmBuilder.BuildIntCast(value, LLVMTypeRef.Int32),
            "f32" => module.LlvmBuilder.BuildIntCast(value, LLVMTypeRef.Float),
            "i8" or "u8" => module.LlvmBuilder.BuildIntCast(value, LLVMTypeRef.Int8),
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
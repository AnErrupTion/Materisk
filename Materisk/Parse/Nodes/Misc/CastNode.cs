using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
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
        _node.Emit(variables, module, type, method, arguments);

        switch (_ident.Text)
        {
            case "int": method.CilMethodBody!.Instructions.Add(CilOpCodes.Conv_I4); break;
            case "float": method.CilMethodBody!.Instructions.Add(CilOpCodes.Conv_R4); break;
            case "byte": method.CilMethodBody!.Instructions.Add(CilOpCodes.Conv_I1); break;
        }

        return null!;
    }

    // TODO: Fix casts from float
    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        var value = (LLVMValueRef)_node.Emit(module, type, method);
        var resultValue = _ident.Text switch
        {
            "int" => module.LlvmBuilder.BuildIntCast(value, LLVMTypeRef.Int32),
            "float" => module.LlvmBuilder.BuildIntCast(value, LLVMTypeRef.Float),
            "byte" => module.LlvmBuilder.BuildIntCast(value, LLVMTypeRef.Int8)
        };
        return resultValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_ident);
        yield return _node;
    }
}
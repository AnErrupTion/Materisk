using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Branch;

internal class CallNode : SyntaxNode
{
    public SyntaxNode ToCallNode { get; }
    private readonly List<SyntaxNode> _argumentNodes;

    public CallNode(SyntaxNode atomNode, List<SyntaxNode> argumentNodes)
    {
        ToCallNode = atomNode;
        _argumentNodes = argumentNodes;
    }

    public override NodeType Type => NodeType.Call;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var toCall = (MateriskMethod)ToCallNode.Emit(module, type, method, metadata);
        var args = EmitArgs(module, type, method, metadata);
        return module.LlvmBuilder.BuildCall2(toCall.Type, toCall.LlvmMethod, args.ToArray());
    }

    public List<LLVMValueRef> EmitArgs(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var args = new List<LLVMValueRef>();

        foreach (var n in _argumentNodes)
        {
            var value = n.Emit(module, type, method, metadata);
            args.Add(value is MateriskUnit unit ? unit.Load() : (LLVMValueRef)value);
        }

        return args;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return ToCallNode;
        foreach (var n in _argumentNodes) yield return n;
    }

    public override string ToString()
    {
        return "CallNode:";
    }
}
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Branch;

internal class CallNode : SyntaxNode
{
    public readonly SyntaxNode ToCallNode;

    private readonly List<SyntaxNode> _argumentNodes;

    public CallNode(SyntaxNode atomNode, List<SyntaxNode> argumentNodes)
    {
        ToCallNode = atomNode;
        _argumentNodes = argumentNodes;
    }

    public override NodeType Type => NodeType.Call;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var toCall = (MateriskMethod)ToCallNode.Emit(module, type, method, metadata);
        var args = EmitArgs(module, type, method, metadata);
        return module.LlvmBuilder.BuildCall2(toCall.Type, toCall.LlvmMethod, args);
    }

    public LLVMValueRef[] EmitArgs(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var args = new LLVMValueRef[_argumentNodes.Count];

        for (var i = 0; i < args.Length; i++)
        {
            var value = _argumentNodes[i].Emit(module, type, method, metadata);
            args[i] = value is MateriskUnit unit ? unit.Load() : (LLVMValueRef)value;
        }

        return args;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return ToCallNode;
        foreach (var n in _argumentNodes) yield return n;
    }
}
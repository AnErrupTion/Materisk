using LLVMSharp.Interop;
using Materisk.TypeSystem;

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

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var toCall = (MateriskMethod)ToCallNode.Emit(module, type, method, thenBlock, elseBlock);
        var args = EmitArgs(module, type, method, thenBlock, elseBlock);
        return module.LlvmBuilder.BuildCall2(toCall.Type, toCall.LlvmMethod, args).ToMateriskValue();
    }

    public LLVMValueRef[] EmitArgs(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var args = new LLVMValueRef[_argumentNodes.Count];

        for (var i = 0; i < args.Length; i++)
        {
            var value = _argumentNodes[i].Emit(module, type, method, thenBlock, elseBlock);
            args[i] = value.Load();
        }

        return args;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return ToCallNode;
        foreach (var n in _argumentNodes) yield return n;
    }
}
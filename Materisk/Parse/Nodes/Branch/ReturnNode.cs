using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Branch;

internal class ReturnNode : SyntaxNode
{
    private readonly SyntaxNode? _returnValueNode;
    
    public ReturnNode(SyntaxNode? returnValueNode = null)
    {
        _returnValueNode = returnValueNode;
    }

    public override NodeType Type => NodeType.Return;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var value = _returnValueNode?.Emit(module, type, method, metadata);

        return (value is not null
            ? module.LlvmBuilder.BuildRet(value.Load())
            : module.LlvmBuilder.BuildRetVoid()).ToMateriskValue();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (_returnValueNode != null) yield return _returnValueNode;
    }
}
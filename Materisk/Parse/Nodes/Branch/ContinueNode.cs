using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Branch;

internal class ContinueNode : SyntaxNode
{
    public override NodeType Type => NodeType.Continue;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        foreach (var obj in metadata.Metadata)
            if (obj is Tuple<MateriskMethod, LLVMBasicBlockRef, LLVMBasicBlockRef> t && t.Item1.Name == method.Name)
                return module.LlvmBuilder.BuildBr(t.Item2).ToMateriskValue();

        throw new InvalidOperationException($"Unable to find \"then\" block for method: {module.Name}.{type.Name}.{method.Name}");
    }
}
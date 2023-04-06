using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Branch;

internal class ContinueNode : SyntaxNode
{
    public override NodeType Type => NodeType.Continue;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        return module.LlvmBuilder.BuildBr(thenBlock).ToMateriskValue();
    }
}
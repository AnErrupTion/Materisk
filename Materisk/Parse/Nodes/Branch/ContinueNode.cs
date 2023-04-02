using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Branch;

internal class ContinueNode : SyntaxNode
{
    public override NodeType Type => NodeType.Continue;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        // Dummy instruction for "for" and "while" nodes
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Leave, new CilInstructionLabel());
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        foreach (var obj in metadata.Metadata)
            if (obj is Tuple<MateriskMethod, LLVMBasicBlockRef, LLVMBasicBlockRef> t && t.Item1.Name == method.Name)
                return module.LlvmBuilder.BuildBr(t.Item2); // Then block

        throw new InvalidOperationException($"Unable to find then block for method: {method.Name}");
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }

    public override string ToString()
    {
        return "ContinueNode:";
    }
}
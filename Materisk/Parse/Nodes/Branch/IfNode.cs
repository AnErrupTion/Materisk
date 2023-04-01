using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;

namespace Materisk.Parse.Nodes.Branch;

internal class IfNode : SyntaxNode
{
    public List<(SyntaxNode cond, SyntaxNode block)> Conditions { get; } = new();

    public override NodeType Type => NodeType.If;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        foreach (var (cond, block) in Conditions)
        {
            cond.Emit(variables, module, type, method, arguments);

            var label = new CilInstructionLabel();
            method.CilMethodBody!.Instructions.Add(CilOpCodes.Brfalse, label);
            block.Emit(variables, module, type, method, arguments);
            method.CilMethodBody!.Instructions.Add(CilOpCodes.Nop);
            label.Instruction = method.CilMethodBody!.Instructions.Last();
        }

        return null!;
    }

    public override object Emit(List<string> variables, LLVMModuleRef module, LLVMValueRef method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (var (cond, block) in Conditions)
        {
            yield return cond;
            yield return block;
        }
    }

    internal void AddCase(SyntaxNode cond, SyntaxNode block)
    {
        Conditions.Add((cond, block));
    }

    public override string ToString()
    {
        return "IfNode:";
    }
}
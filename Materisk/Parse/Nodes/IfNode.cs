using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parse.Nodes;

internal class IfNode : SyntaxNode
{
    public List<(SyntaxNode cond, SyntaxNode block)> Conditions { get; } = new();

    public override NodeType Type => NodeType.If;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        foreach (var (cond, block) in Conditions)
        {
            cond.Emit(variables, module, type, method, arguments);
            var label = new CilInstructionLabel();
            method.CilMethodBody.Instructions.Add(CilOpCodes.Brfalse, label);
            var index = (int)method.CilMethodBody.Instructions.IndexOf(method.CilMethodBody.Instructions.Last());
            block.Emit(variables, module, type, method, arguments);
            method.CilMethodBody.Instructions.Add(CilOpCodes.Nop);
            label.Instruction = method.CilMethodBody.Instructions.Last();
            method.CilMethodBody.Instructions[index].Operand = label;
        }

        return null;
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
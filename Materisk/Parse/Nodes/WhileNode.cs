using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Materisk.Parse.Nodes;

internal class WhileNode : SyntaxNode
{
    private readonly SyntaxNode _condNode;
    private readonly SyntaxNode _block;

    public WhileNode(SyntaxNode condNode, SyntaxNode block)
    {
        _condNode = condNode;
        _block = block;
    }

    public override NodeType Type => NodeType.While;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var index = method.CilMethodBody!.Instructions.Count;
        var condLabel = new CilInstructionLabel();
        var blockStartLabel = new CilInstructionLabel();

        method.CilMethodBody!.Instructions.Add(CilOpCodes.Br, condLabel);

        _block.Emit(variables, module, type, method, arguments);
        blockStartLabel.Instruction = method.CilMethodBody!.Instructions[index + 1];

        index = method.CilMethodBody!.Instructions.Count;
        _condNode.Emit(variables, module, type, method, arguments);
        condLabel.Instruction = method.CilMethodBody!.Instructions[index];

        method.CilMethodBody!.Instructions.Add(CilOpCodes.Brtrue, blockStartLabel);
        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _condNode;
        yield return _block;
    }
}
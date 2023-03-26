using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Materisk.Parse.Nodes;

internal class ForNode : SyntaxNode
{
    private readonly SyntaxNode _initialExpressionNode;
    private readonly SyntaxNode _condNode;
    private readonly SyntaxNode _stepNode;
    private readonly SyntaxNode _block;

    public ForNode(SyntaxNode initialExpressionNode, SyntaxNode condNode, SyntaxNode stepNode, SyntaxNode block)
    {
        _initialExpressionNode = initialExpressionNode;
        _condNode = condNode;
        _stepNode = stepNode;
        _block = block;
    }

    public override NodeType Type => NodeType.For;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        _initialExpressionNode.Emit(variables, module, type, method, arguments);

        var index = method.CilMethodBody!.Instructions.Count;
        var condLabel = new CilInstructionLabel();
        var stepLabel = new CilInstructionLabel();

        method.CilMethodBody!.Instructions.Add(CilOpCodes.Br, condLabel);

        _stepNode.Emit(variables, module, type, method, arguments);
        stepLabel.Instruction = method.CilMethodBody!.Instructions[index + 1];

        _block.Emit(variables, module, type, method, arguments);

        index = method.CilMethodBody!.Instructions.Count;
        _condNode.Emit(variables, module, type, method, arguments);
        condLabel.Instruction = method.CilMethodBody!.Instructions[index];

        method.CilMethodBody!.Instructions.Add(CilOpCodes.Brtrue, stepLabel);

        return null!;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _initialExpressionNode;
        yield return _condNode;
        yield return _stepNode;
        yield return _block;
    }
}
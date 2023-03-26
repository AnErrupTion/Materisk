using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parse.Nodes;

internal class ForNode : SyntaxNode
{
    private readonly SyntaxNode initialExpressionNode;
    private readonly SyntaxNode condNode;
    private readonly SyntaxNode stepNode;
    private readonly SyntaxNode block;

    public ForNode(SyntaxNode initialExpressionNode, SyntaxNode condNode, SyntaxNode stepNode, SyntaxNode block)
    {
        this.initialExpressionNode = initialExpressionNode;
        this.condNode = condNode;
        this.stepNode = stepNode;
        this.block = block;
    }

    public override NodeType Type => NodeType.For;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        initialExpressionNode.Emit(variables, module, type, method, arguments);

        var index = method.CilMethodBody.Instructions.Count;
        var condLabel = new CilInstructionLabel();
        var stepLabel = new CilInstructionLabel();

        method.CilMethodBody.Instructions.Add(CilOpCodes.Br, condLabel);

        stepNode.Emit(variables, module, type, method, arguments);
        stepLabel.Instruction = method.CilMethodBody.Instructions[index + 1];

        block.Emit(variables, module, type, method, arguments);

        index = method.CilMethodBody.Instructions.Count;

        condNode.Emit(variables, module, type, method, arguments);
        condLabel.Instruction = method.CilMethodBody.Instructions[index + 1];

        method.CilMethodBody.Instructions.Add(CilOpCodes.Brtrue, stepLabel);

        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return initialExpressionNode;
        yield return condNode;
        yield return stepNode;
        yield return block;
    }
}
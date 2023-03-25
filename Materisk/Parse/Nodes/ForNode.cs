using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
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
        Scope forScope = new(scope);
        var lastVal = SValue.Null;
        initialExpressionNode.Evaluate(forScope);

        while (true)
        {
            if (!condNode.Evaluate(forScope).IsTruthy()) break;
            var forBlockRes = block.Evaluate(forScope);
            if (!forBlockRes.IsNull()) lastVal = forBlockRes;

            if (forScope.State == ScopeState.ShouldBreak) break;
            if (forScope.State != ScopeState.None) forScope.SetState(ScopeState.None);

            stepNode.Evaluate(forScope);
        }

        return lastVal;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return initialExpressionNode;
        yield return condNode;
        yield return stepNode;
        yield return block;
    }
}
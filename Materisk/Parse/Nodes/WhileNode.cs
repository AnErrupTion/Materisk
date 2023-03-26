using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

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

    public override SValue Evaluate(Scope scope)
    {
        Scope whileScope = new(scope);
        var lastVal = SValue.Null;

        while (true)
        {
            if (!_condNode.Evaluate(whileScope).IsTruthy()) break;
            var whileBlockRes = _block.Evaluate(whileScope);
            if (!whileBlockRes.IsNull()) lastVal = whileBlockRes;

            if (whileScope.State == ScopeState.ShouldBreak) break;
            if (whileScope.State != ScopeState.None) whileScope.SetState(ScopeState.None);
        }

        return lastVal;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _condNode;
        yield return _block;
    }
}
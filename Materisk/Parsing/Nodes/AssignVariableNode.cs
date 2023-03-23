using AsmResolver.DotNet;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class AssignVariableNode : SyntaxNode
{
    public AssignVariableNode(SyntaxToken ident, SyntaxNode expr)
    {
        Ident = ident;
        Expr = expr;
    }

    public override NodeType Type => NodeType.AssignVariable;

    public SyntaxToken Ident { get; }

    public SyntaxNode Expr { get; }

    public override SValue Evaluate(Scope scope)
    {
        if (scope.Get(Ident.Value.ToString()) == null)
        {
            throw new InvalidOperationException("Can not assign to a non-existant identifier");
        }

        var val = Expr.Evaluate(scope);
        var key = Ident.Value.ToString();
        if (!scope.Update(key, val, out var ex)) throw ex;
        return val;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(Ident);
        yield return Expr;
    }

    public override string ToString()
    {
        return "AssignVariableNode:";
    }
}
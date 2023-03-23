using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
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

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var name = Ident.Value.ToString();

        if (name is null || !variables.ContainsKey(name))
            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

        var value = Expr.Emit(variables, module, method, arguments);
        var variable = variables[name];
        method.CilMethodBody?.Instructions.Add(CilOpCodes.Stloc, variable);
        return value;
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
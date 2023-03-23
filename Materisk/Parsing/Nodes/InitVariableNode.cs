using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class InitVariableNode : SyntaxNode
{
    private readonly SyntaxToken ident;
    private readonly SyntaxToken type;
    private readonly SyntaxNode expr;

    public InitVariableNode(SyntaxToken ident, SyntaxToken type, SyntaxNode expr)
    {
        this.ident = ident;
        this.type = type;
        this.expr = expr;
    }

    public override NodeType Type => NodeType.InitVariable;

    public override SValue Evaluate(Scope scope)
    {
        if (scope.Get(ident.Value.ToString()) != null)
        {
            throw new InvalidOperationException("Can not initiliaze the same variable twice!");
        }

        if (expr != null)
        {
            var val = expr.Evaluate(scope);
            val.TypeIsFixed = true;

            scope.Set(ident.Value.ToString(), val);
            return val;
        }

        scope.Set(ident.Value.ToString(), SValue.Null);
        return SValue.Null;

    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var name = ident.Value.ToString();

        if (name is null)
            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

        if (variables.ContainsKey(name))
            throw new InvalidOperationException("Can not initialize the same variable twice!");

        if (expr == null)
            throw new InvalidOperationException("Variable initialization needs an expression!");

        var value = expr.Emit(variables, module, method, arguments);
        var variable = new CilLocalVariable(Utils.GetTypeSignatureFor(module, type.Value.ToString()));
        method.CilMethodBody?.LocalVariables.Add(variable);
        method.CilMethodBody?.Instructions.Add(CilOpCodes.Stloc, variable);
        variables.Add(name, variable);
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(ident);
        if (expr != null) yield return expr;
    }

    public override string ToString()
    {
        return "InitVariableNode:";
    }
}
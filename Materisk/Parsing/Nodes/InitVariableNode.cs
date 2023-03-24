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
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var name = ident.Value.ToString();

        if (name is null)
            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

        if (variables.ContainsKey(name))
            throw new InvalidOperationException("Can not initialize the same variable twice!");

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
        yield return expr;
    }

    public override string ToString()
    {
        return "InitVariableNode:";
    }
}
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class InitVariableNode : SyntaxNode
{
    private readonly SyntaxToken identToken;
    private readonly SyntaxToken typeToken;
    private readonly SyntaxToken? secondTypeToken;
    private readonly SyntaxNode expr;

    public InitVariableNode(SyntaxToken identToken, SyntaxToken typeToken, SyntaxToken? secondTypeToken, SyntaxNode expr)
    {
        this.identToken = identToken;
        this.typeToken = typeToken;
        this.secondTypeToken = secondTypeToken;
        this.expr = expr;
    }

    public override NodeType Type => NodeType.InitVariable;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var name = identToken.Text;

        if (name is null)
            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

        if (variables.ContainsKey(name))
            throw new InvalidOperationException("Can not initialize the same variable twice!");

        var value = expr.Emit(variables, module, type, method, arguments);
        var variable = new CilLocalVariable(typeToken.Text is "arr" && secondTypeToken is not null ? Utils.GetTypeSignatureFor(module, secondTypeToken.Text, true) : Utils.GetTypeSignatureFor(module, typeToken.Text));
        method.CilMethodBody.LocalVariables.Add(variable);
        method.CilMethodBody.Instructions.Add(CilOpCodes.Stloc, variable);
        variables.Add(name, variable);
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(identToken);
        yield return expr;
    }

    public override string ToString()
    {
        return "InitVariableNode:";
    }
}
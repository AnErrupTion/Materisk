using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class InitVariableNode : SyntaxNode
{
    private readonly SyntaxToken _identToken;
    private readonly SyntaxToken _typeToken;
    private readonly SyntaxToken? _secondTypeToken;
    private readonly SyntaxNode _expr;

    public InitVariableNode(SyntaxToken identToken, SyntaxToken typeToken, SyntaxToken? secondTypeToken, SyntaxNode expr)
    {
        _identToken = identToken;
        _typeToken = typeToken;
        _secondTypeToken = secondTypeToken;
        _expr = expr;
    }

    public override NodeType Type => NodeType.InitVariable;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var name = _identToken.Text;

        if (name is null)
            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

        if (variables.ContainsKey(name))
            throw new InvalidOperationException("Can not initialize the same variable twice!");

        var value = _expr.Emit(variables, module, type, method, arguments);
        var variable = new CilLocalVariable(_typeToken.Text is "arr" && _secondTypeToken is not null ? Utils.GetTypeSignatureFor(module, _secondTypeToken.Text, true) : Utils.GetTypeSignatureFor(module, _typeToken.Text));
        method.CilMethodBody.LocalVariables.Add(variable);
        method.CilMethodBody.Instructions.Add(CilOpCodes.Stloc, variable);
        variables.Add(name, variable);
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_identToken);
        yield return _expr;
    }

    public override string ToString()
    {
        return "InitVariableNode:";
    }
}
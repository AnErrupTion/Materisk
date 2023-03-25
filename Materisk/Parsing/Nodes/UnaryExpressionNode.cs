using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class UnaryExpressionNode : SyntaxNode
{
    private readonly SyntaxToken token;
    private readonly SyntaxNode rhs;

    public UnaryExpressionNode(SyntaxToken token, SyntaxNode rhs)
    {
        this.token = token;
        this.rhs = rhs;
    }

    public override NodeType Type => NodeType.UnaryExpression;

    public override SValue Evaluate(Scope scope)
    {
        switch (token.Type)
        {
            case SyntaxType.Bang: return rhs.Evaluate(scope).Not();
            case SyntaxType.Minus: return rhs.Evaluate(scope).ArithNot();
            case SyntaxType.Plus: return rhs.Evaluate(scope);
            default: throw new InvalidOperationException();
        }
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(token);
        yield return rhs;
    }

    public override string ToString()
    {
        return "UnaryExpressionNode:";
    }
}
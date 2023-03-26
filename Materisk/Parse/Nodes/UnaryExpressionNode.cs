using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

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
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        rhs.Emit(variables, module, type, method, arguments);

        switch (token.Type)
        {
            case SyntaxType.Bang:
                method.CilMethodBody.Instructions.Add(CilOpCodes.Ldc_I4_0);
                method.CilMethodBody.Instructions.Add(CilOpCodes.Ceq);
                break;
            case SyntaxType.Minus:
                method.CilMethodBody.Instructions.Add(CilOpCodes.Neg);
                break;
            case SyntaxType.Plus:
                break;
            default:
                throw new InvalidOperationException($"Trying to do a unary expression on: {token.Type}");
        }

        return null;
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
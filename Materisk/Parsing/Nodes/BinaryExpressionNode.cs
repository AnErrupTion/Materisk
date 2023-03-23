using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class BinaryExpressionNode : SyntaxNode
{
    private readonly SyntaxNode left;
    private readonly SyntaxToken operatorToken;
    private readonly SyntaxNode right;

    public BinaryExpressionNode(SyntaxNode left, SyntaxToken operatorToken, SyntaxNode right)
    {
        this.left = left;
        this.operatorToken = operatorToken;
        this.right = right;
    }

    public override NodeType Type => NodeType.BinaryExpression;

    public override SValue Evaluate(Scope scope)
    {
        var leftRes = left.Evaluate(scope);
        var rightRes = right.Evaluate(scope);

        return operatorToken.Type switch
        {
            SyntaxType.Plus => leftRes.Add(rightRes),
            SyntaxType.Minus => leftRes.Sub(rightRes),
            SyntaxType.Div => leftRes.Div(rightRes),
            SyntaxType.Mul => leftRes.Mul(rightRes),
            SyntaxType.Mod => leftRes.Mod(rightRes),
            SyntaxType.EqualsEquals => leftRes.Equals(rightRes),
            SyntaxType.Idx => leftRes.Idx(rightRes),
            SyntaxType.LessThan => leftRes.LessThan(rightRes),
            SyntaxType.LessThanEqu => leftRes.LessThanEqu(rightRes),
            SyntaxType.GreaterThan => leftRes.GreaterThan(rightRes),
            SyntaxType.GreaterThanEqu => leftRes.GreaterThanEqu(rightRes),
            _ => throw new NotImplementedException()
        };
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, Dictionary<string, object> arguments)
    {
        /*var leftType = left.Emit(module, body);
        var rightType = right.Emit(module, body);

        switch (operatorToken.Type)
        {
            case SyntaxType.Plus:
                body.Instructions.Add(CilOpCodes.Add);
                return leftType;
            case SyntaxType.Minus:
                body.Instructions.Add(CilOpCodes.Sub);
                return leftType;
            case SyntaxType.Div:
                body.Instructions.Add(CilOpCodes.Div);
                return leftType;
            case SyntaxType.Mul:
                body.Instructions.Add(CilOpCodes.Mul);
                return leftType;
            case SyntaxType.Mod:
                body.Instructions.Add(CilOpCodes.Rem);
                return leftType;
            case SyntaxType.EqualsEquals:
                body.Instructions.Add(CilOpCodes.Ceq);
                return SBuiltinType.Int;
            case SyntaxType.Idx:
                // TODO: Correct type
                body.Instructions.Add(CilOpCodes.Ldelem);
                return SBuiltinType.Null;
            case SyntaxType.LessThan:
                body.Instructions.Add(CilOpCodes.Clt);
                return SBuiltinType.Int;
            case SyntaxType.LessThanEqu:
                body.Instructions.Add(CilOpCodes.Cgt);
                body.Instructions.Add(CilOpCodes.Ldc_I4_0);
                body.Instructions.Add(CilOpCodes.Ceq);
                return SBuiltinType.Int;
            case SyntaxType.GreaterThan:
                body.Instructions.Add(CilOpCodes.Cgt);
                return SBuiltinType.Int;
            case SyntaxType.GreaterThanEqu:
                body.Instructions.Add(CilOpCodes.Clt);
                body.Instructions.Add(CilOpCodes.Ldc_I4_0);
                body.Instructions.Add(CilOpCodes.Ceq);
                return SBuiltinType.Int;

            default:
                throw new NotImplementedException();
        }*/

        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return left;
        yield return new TokenNode(operatorToken);
        yield return right;
    }

    public override string ToString()
    {
        return "BinaryExprNode: op=" + operatorToken.Type;
    }
}
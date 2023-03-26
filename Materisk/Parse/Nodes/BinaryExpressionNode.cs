using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class BinaryExpressionNode : SyntaxNode
{
    private readonly SyntaxNode _left;
    private readonly SyntaxToken _operatorToken;
    private readonly SyntaxNode _right;

    public BinaryExpressionNode(SyntaxNode left, SyntaxToken operatorToken, SyntaxNode right)
    {
        _left = left;
        _operatorToken = operatorToken;
        _right = right;
    }

    public override NodeType Type => NodeType.BinaryExpression;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        _left.Emit(variables, module, type, method, arguments);
        _right.Emit(variables, module, type, method, arguments);

        switch (_operatorToken.Type)
        {
            case SyntaxType.Plus: method.CilMethodBody.Instructions.Add(CilOpCodes.Add); break;
            case SyntaxType.Minus: method.CilMethodBody.Instructions.Add(CilOpCodes.Sub); break;
            case SyntaxType.Div: method.CilMethodBody.Instructions.Add(CilOpCodes.Div); break;
            case SyntaxType.Mul: method.CilMethodBody.Instructions.Add(CilOpCodes.Mul); break;
            case SyntaxType.Mod: method.CilMethodBody.Instructions.Add(CilOpCodes.Rem); break;
            case SyntaxType.EqualsEquals: method.CilMethodBody.Instructions.Add(CilOpCodes.Ceq); break;
            case SyntaxType.LessThan: method.CilMethodBody.Instructions.Add(CilOpCodes.Clt); break;
            case SyntaxType.LessThanEqu:
            {
                method.CilMethodBody.Instructions.Add(CilOpCodes.Cgt);
                method.CilMethodBody.Instructions.Add(CilOpCodes.Ldc_I4_0);
                method.CilMethodBody.Instructions.Add(CilOpCodes.Ceq);
                break;
            }
            case SyntaxType.GreaterThan: method.CilMethodBody.Instructions.Add(CilOpCodes.Cgt); break;
            case SyntaxType.GreaterThanEqu:
            {
                method.CilMethodBody.Instructions.Add(CilOpCodes.Clt);
                method.CilMethodBody.Instructions.Add(CilOpCodes.Ldc_I4_0);
                method.CilMethodBody.Instructions.Add(CilOpCodes.Ceq);
                break;
            }
            default: throw new InvalidOperationException($"Trying to do a binary expression on: {_operatorToken.Type}");
        }

        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _left;
        yield return new TokenNode(_operatorToken);
        yield return _right;
    }

    public override string ToString()
    {
        return "BinaryExprNode: op=" + _operatorToken.Type;
    }
}
﻿using AsmResolver.DotNet;
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
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        left.Emit(variables, module, method, arguments);
        right.Emit(variables, module, method, arguments);

        switch (operatorToken.Type)
        {
            case SyntaxType.Plus: method.CilMethodBody?.Instructions.Add(CilOpCodes.Add); break;
            case SyntaxType.Minus: method.CilMethodBody?.Instructions.Add(CilOpCodes.Sub); break;
            case SyntaxType.Div: method.CilMethodBody?.Instructions.Add(CilOpCodes.Div); break;
            case SyntaxType.Mul: method.CilMethodBody?.Instructions.Add(CilOpCodes.Mul); break;
            case SyntaxType.Mod: method.CilMethodBody?.Instructions.Add(CilOpCodes.Rem); break;
            case SyntaxType.EqualsEquals: method.CilMethodBody?.Instructions.Add(CilOpCodes.Ceq); break;
            case SyntaxType.Idx:
            {
                throw new NotImplementedException();
            }
            case SyntaxType.LessThan: method.CilMethodBody?.Instructions.Add(CilOpCodes.Clt); break;
            case SyntaxType.LessThanEqu:
            {
                method.CilMethodBody?.Instructions.Add(CilOpCodes.Cgt);
                method.CilMethodBody?.Instructions.Add(CilOpCodes.Ldc_I4_0);
                method.CilMethodBody?.Instructions.Add(CilOpCodes.Ceq);
                break;
            }
            case SyntaxType.GreaterThan: method.CilMethodBody?.Instructions.Add(CilOpCodes.Cgt); break;
            case SyntaxType.GreaterThanEqu:
            {
                method.CilMethodBody?.Instructions.Add(CilOpCodes.Clt);
                method.CilMethodBody?.Instructions.Add(CilOpCodes.Ldc_I4_0);
                method.CilMethodBody?.Instructions.Add(CilOpCodes.Ceq);
                break;
            }
            default: throw new NotImplementedException();
        }

        return null;
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
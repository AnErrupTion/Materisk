using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class AssignExpressionNode : SyntaxNode
{
    public AssignExpressionNode(SyntaxToken ident, SyntaxNode expr)
    {
        Ident = ident;
        Expr = expr;
    }

    public override NodeType Type => NodeType.AssignExpression;

    public SyntaxToken Ident { get; }

    public SyntaxNode Expr { get; }

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var name = Ident.Text;

        if (name is null || !variables.ContainsKey(name))
        {
            if (method.DeclaringType != null)
            {
                foreach (var field in method.DeclaringType.Fields)
                    if (field.Name == name)
                    {
                        if (!field.IsStatic)
                            throw new InvalidOperationException($"Field \"{name}\" is not static.");

                        var fieldValue = Expr.Emit(variables, module, type, method, arguments);
                        method.CilMethodBody!.Instructions.Add(CilOpCodes.Stsfld, field);
                        return fieldValue;
                    }
            }

            throw new InvalidOperationException("Can not assign to a non-existent identifier!");
        }

        var varValue = Expr.Emit(variables, module, type, method, arguments);
        var variable = variables[name];
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Stloc, variable);
        return varValue;
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
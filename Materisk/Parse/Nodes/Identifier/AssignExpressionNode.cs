using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Identifier;

internal class AssignExpressionNode : SyntaxNode
{
    public AssignExpressionNode(string identifier, SyntaxNode expr)
    {
        Identifier = identifier;
        Expr = expr;
    }

    public override NodeType Type => NodeType.AssignExpression;

    public readonly string Identifier;

    public readonly SyntaxNode Expr;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var variable = method.GetVariableByName(Identifier);

        if (variable is null)
        {
            foreach (var field in type.Fields)
                if (field.Name == Identifier)
                {
                    if (!field.Attributes.HasFlag(MateriskAttributes.Static))
                        throw new InvalidOperationException($"Field \"{module.Name}.{type.Name}.{Identifier}\" is not static.");

                    var fieldValue = Expr.Emit(module, type, method, metadata);
                    field.Store(fieldValue.Load());
                    return fieldValue;
                }

            throw new InvalidOperationException($"Can not assign to a non-existent identifier \"{Identifier}\" in method: {module.Name}.{type.Name}.{method.Name}");
        }

        var varValue = Expr.Emit(module, type, method, metadata);
        variable.Store(varValue.Load());
        return varValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Expr;
    }
}
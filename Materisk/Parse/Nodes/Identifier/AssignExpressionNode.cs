using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

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

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var variable = method.GetVariableByName(Identifier);

        if (variable is null)
        {
            foreach (var field in method.ParentType.Fields)
                if (field.Name == Identifier)
                {
                    // TODO: Non-static fields
                    /*if (!field.IsStatic)
                        throw new InvalidOperationException($"Field \"{name}\" is not static.");*/

                    var fieldValue = Expr.Emit(module, type, method, metadata);
                    field.Store(fieldValue is MateriskUnit fieldUnit ? fieldUnit.Load() : (LLVMValueRef)fieldValue);
                    return fieldValue;
                }

            throw new InvalidOperationException("Can not assign to a non-existent identifier!");
        }

        var varValue = Expr.Emit(module, type, method, metadata);
        variable.Store(varValue is MateriskUnit varUnit ? varUnit.Load() : (LLVMValueRef)varValue);
        return varValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Expr;
    }
}
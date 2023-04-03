using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;

namespace Materisk.Parse.Nodes.Identifier;

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

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var name = Ident.Text;
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

        var variable = method.GetVariableByName(name);

        if (variable is null)
        {
            foreach (var field in method.ParentType.Fields)
                if (field.Name == name)
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
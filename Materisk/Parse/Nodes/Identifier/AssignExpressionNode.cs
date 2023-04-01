using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;

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

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
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

                    var fieldValue = (LLVMValueRef)Expr.Emit(module, type, method);
                    module.LlvmBuilder.BuildStore(fieldValue, field.LlvmField);
                    return fieldValue;
                }

            throw new InvalidOperationException("Can not assign to a non-existent identifier!");
        }

        if (!variable.Mutable)
            throw new InvalidOperationException("Can not assign to an immutable variable!");

        var varValue = (LLVMValueRef)Expr.Emit(module, type, method);
        module.LlvmBuilder.BuildStore(varValue, variable.Value);
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
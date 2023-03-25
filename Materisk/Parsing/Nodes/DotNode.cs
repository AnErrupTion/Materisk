using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class DotNode : SyntaxNode
{
    public DotNode(SyntaxNode callNode)
    {
        CallNode = callNode;
    }

    public SyntaxNode CallNode { get; }
    public List<SyntaxNode> NextNodes { get; } = new();

    public override NodeType Type => NodeType.Dot;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var currentValue = CallNode.Emit(variables, module, type, method, arguments);

        foreach (var node in NextNodes)
            switch (node)
            {
                case IdentifierNode rvn:
                {
                    var name = rvn.Token.Text;
                    var typeName = currentValue is CilLocalVariable variable ? variable.VariableType.ToString() : method.DeclaringType?.ToString();

                    FieldDefinition? fieldDef = null;

                    foreach (var typeDef in module.TopLevelTypes)
                        foreach (var field in typeDef.Fields)
                            if (typeDef.FullName == typeName && field.Name == name)
                            {
                                fieldDef = field;
                                break;
                            }

                    if (fieldDef == null)
                    {
                        typeName = currentValue.ToString();

                        var index = 0;
                        var found = false;

                        foreach (var argument in arguments)
                        {
                            if (argument == typeName)
                            {
                                typeName = method.Parameters[index].ParameterType.Name;
                                found = true;
                                break;
                            }

                            index++;
                        }

                        if (!found)
                            throw new InvalidOperationException($"Unable to find type for argument: {typeName}");

                        foreach (var typeDef in module.TopLevelTypes)
                            foreach (var field in typeDef.Fields)
                                if (typeDef.Name == typeName && field.Name == name)
                                {
                                    fieldDef = field;
                                    break;
                                }

                        if (fieldDef is null)
                            throw new InvalidOperationException($"Unable to find field with name: {name}");
                    }

                    method.CilMethodBody?.Instructions.Add(fieldDef.IsStatic ? CilOpCodes.Ldsfld : CilOpCodes.Ldfld, fieldDef);
                    currentValue = fieldDef;
                    break;
                }
                case AssignExpressionNode aen:
                {
                    var name = aen.Ident.Text;
                    var typeName = currentValue is CilLocalVariable variable ? variable.VariableType.ToString() : method.DeclaringType?.ToString();

                    aen.Expr.Emit(variables, module, type, method, arguments);

                    FieldDefinition? fieldDef = null;

                    foreach (var typeDef in module.TopLevelTypes)
                        foreach (var field in typeDef.Fields)
                            if (typeDef.FullName == typeName && field.Name == name)
                            {
                                fieldDef = field;
                                break;
                            }

                    if (fieldDef == null)
                    {
                        typeName = currentValue.ToString();

                        var index = 0;
                        var found = false;

                        foreach (var argument in arguments)
                        {
                            if (argument == typeName)
                            {
                                typeName = method.Parameters[index].ParameterType.Name;
                                found = true;
                                break;
                            }

                            index++;
                        }

                        if (!found)
                            throw new InvalidOperationException($"Unable to find type for argument: {typeName}");

                        foreach (var typeDef in module.TopLevelTypes)
                            foreach (var field in typeDef.Fields)
                                if (typeDef.Name == typeName && field.Name == name)
                                {
                                    fieldDef = field;
                                    break;
                                }

                        if (fieldDef is null)
                            throw new InvalidOperationException($"Unable to find field with name: {name}");
                    }

                    method.CilMethodBody?.Instructions.Add(fieldDef.IsStatic ? CilOpCodes.Stsfld : CilOpCodes.Stfld, fieldDef);
                    break;
                }
                case CallNode { ToCallNode: IdentifierNode cnIdentNode } cn:
                {
                    var name = cnIdentNode.Token.Text;
                    var typeName = currentValue is CilLocalVariable variable ? variable.VariableType.ToString() : currentValue.ToString();

                    MethodDefinition? newMethod = null;
                    foreach (var typeDef in module.TopLevelTypes)
                        foreach (var meth in typeDef.Methods)
                            if (typeDef.FullName == typeName && meth.Name == name)
                            {
                                newMethod = meth;
                                break;
                            }

                    if (newMethod == null)
                        throw new InvalidOperationException($"Unable to find method with name: {name}");

                    cn.EmitArgs(variables, module, type, method, arguments);

                    method.CilMethodBody?.Instructions.Add(CilOpCodes.Call, newMethod);
                    break;
                }
                case CallNode _:
                    throw new Exception("Tried to call a non identifier in dot node stack.");
                default:
                    throw new Exception("Unexpected node in dot node stack!");
            }

        return currentValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return CallNode;

        foreach (var node in NextNodes) yield return node;
    }

    public override string ToString()
    {
        return "DotNode:";
    }
}
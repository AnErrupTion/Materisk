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
        var currentValue = CallNode.Evaluate(scope);

        foreach (var node in NextNodes)
        {
            if (node is IdentifierNode rvn)
            {
                var ident = rvn.Token;
                currentValue = currentValue.Dot(new SString((string)ident.Value));
            }else if(node is AssignExpressionNode aen) {
                var ident = aen.Ident;
                return currentValue.DotAssignment(new SString(ident.Text), aen.Expr.Evaluate(scope));
            }
            else if (node is CallNode cn)
            {
                if (cn.ToCallNode is IdentifierNode cnIdentNode)
                {
                    var ident = cnIdentNode.Token;
                    var lhs = currentValue.Dot(new SString((string)ident.Value));

                    var args = cn.EvaluateArgs(scope);
                    if (lhs is SBaseFunction { IsClassInstanceMethod: true } func) {
                        var idxOfSelf = func.ExpectedArgs.IndexOf("self");
                        if(idxOfSelf != -1) args.Insert(idxOfSelf, currentValue);
                    }

                    currentValue = lhs.Call(scope, args);
                }
                else
                {
                    throw new Exception("Tried to call a non identifier in dot node stack.");
                }
            }
            else
            {
                throw new Exception("Unexpected node in dot node stack!");
            }
        }

        return currentValue;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var currentValue = CallNode.Emit(variables, module, method, arguments);

        foreach (var node in NextNodes)
            switch (node)
            {
                case IdentifierNode rvn:
                {
                    var name = rvn.Token.Text;
                    var typeName = method.Parameters[0].ParameterType.Name;

                    FieldDefinition? fieldDef = null;

                    foreach (var type in module.TopLevelTypes)
                        foreach (var field in type.Fields)
                            if (type.Name == typeName && field.Name == name)
                            {
                                fieldDef = field;
                                break;
                            }

                    if (fieldDef == null)
                        throw new InvalidOperationException($"Unable to find field with name: {name}");

                    method.CilMethodBody?.Instructions.Add(CilOpCodes.Ldfld, fieldDef);
                    currentValue = fieldDef;
                    break;
                }
                case AssignExpressionNode aen:
                {
                    var name = aen.Ident.Text;
                    var typeName = method.Parameters[0].ParameterType.Name;

                    aen.Expr.Emit(variables, module, method, arguments);

                    FieldDefinition? fieldDef = null;

                    foreach (var type in module.TopLevelTypes)
                        foreach (var field in type.Fields)
                            if (type.Name == typeName && field.Name == name)
                            {
                                fieldDef = field;
                                break;
                            }

                    if (fieldDef == null)
                        throw new InvalidOperationException($"Unable to find field with name: {name}");

                    method.CilMethodBody?.Instructions.Add(CilOpCodes.Stfld, fieldDef);
                    break;
                }
                case CallNode { ToCallNode: IdentifierNode cnIdentNode } cn:
                {
                    var ident = cnIdentNode.Token;
                    var typeName = currentValue.ToString();
                    var name = ident.Value.ToString();

                    MethodDefinition newMethod = null;
                    foreach (var type in module.TopLevelTypes)
                        foreach (var meth in type.Methods)
                            if (type.FullName == typeName && meth.Name == name)
                            {
                                newMethod = meth;
                                break;
                            }

                    if (newMethod == null)
                        throw new InvalidOperationException($"Unable to find method with name: {name}");

                    cn.EmitArgs(variables, module, method, arguments);

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
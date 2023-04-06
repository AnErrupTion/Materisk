using LLVMSharp.Interop;
using Materisk.Parse.Nodes.Branch;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Identifier;

internal class DotNode : SyntaxNode
{
    private readonly SyntaxNode _callNode;

    public readonly List<SyntaxNode> NextNodes;

    public DotNode(SyntaxNode callNode)
    {
        _callNode = callNode;

        NextNodes = new();
    }

    public override NodeType Type => NodeType.Dot;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var currentValue = _callNode.Emit(module, type, method, thenBlock, elseBlock);

        foreach (var node in NextNodes)
            switch (node)
            {
                case IdentifierNode rvn:
                {
                    var name = rvn.Name;
                    var typeName = currentValue switch
                    {
                        MateriskLocalVariable variable => variable.ParentMethod.ParentType.Name,
                        MateriskType mType => mType.Name,
                        _ => method.ParentType.Name
                    };

                    MateriskField? mField = null;

                    foreach (var typeDef in module.Types)
                        foreach (var field in typeDef.Fields)
                            if (typeDef.Name == typeName && field.Name == name)
                            {
                                mField = field;
                                break;
                            }

                    if (mField is null)
                    {
                        // TODO?
                        /*typeName = currentValue.ToString();

                        var found = false;

                        foreach (var argument in method.Arguments)
                            if (argument.Name == typeName)
                            {
                                typeName = argument.ParentMethod.ParentType.Name;
                                found = true;
                                break;
                            }

                        if (!found)
                            throw new InvalidOperationException($"Unable to find type for argument: {typeName}");

                        foreach (var typeDef in module.Types)
                            foreach (var field in typeDef.Fields)
                                if (typeDef.Name == typeName && field.Name == name)
                                {
                                    mField = field;
                                    break;
                                }

                        if (mField is null)*/
                            throw new InvalidOperationException($"Unable to find field with name \"{name}\" in module: {module.Name}");
                    }

                    currentValue = mField;
                    break;
                }
                case AssignExpressionNode aen:
                {
                    var name = aen.Identifier;
                    var typeName = currentValue switch
                    {
                        MateriskLocalVariable variable => variable.ParentMethod.ParentType.Name,
                        MateriskType mType => mType.Name,
                        _ => method.ParentType.Name
                    };
                    var value = aen.Expr.Emit(module, type, method, thenBlock, elseBlock);

                    MateriskField? mField = null;

                    foreach (var typeDef in module.Types)
                        foreach (var field in typeDef.Fields)
                            if (typeDef.Name == typeName && field.Name == name)
                            {
                                mField = field;
                                break;
                            }

                    if (mField is null)
                    {
                        // TODO?
                        /*typeName = currentValue.ToString();

                        var found = false;

                        foreach (var argument in method.Arguments)
                            if (argument.Name == typeName)
                            {
                                typeName = argument.ParentMethod.ParentType.Name;
                                found = true;
                                break;
                            }

                        if (!found)
                            throw new InvalidOperationException($"Unable to find type for argument: {typeName}");

                        foreach (var typeDef in module.Types)
                            foreach (var field in typeDef.Fields)
                                if (typeDef.Name == typeName && field.Name == name)
                                {
                                    mField = field;
                                    break;
                                }

                        if (mField is null)*/
                        throw new InvalidOperationException($"Unable to find field with name \"{name}\" in module: {module.Name}");
                    }

                    // TODO: Non-static fields
                    mField.Store(value.Load());
                    break;
                }
                case CallNode { ToCallNode: IdentifierNode cnIdentNode } cn:
                {
                    var name = cnIdentNode.Name;
                    var typeName = currentValue is MateriskType mType ? mType.Name : currentValue.ToString();
    
                    MateriskMethod? newMethod = null;
                    foreach (var typeDef in module.Types)
                        foreach (var meth in typeDef.Methods)
                            if (typeDef.Name == typeName && meth.Name == name)
                            {
                                newMethod = meth;
                                break;
                            }

                    if (newMethod == null)
                        throw new InvalidOperationException($"Unable to find method with name \"{name}\" in module: {module.Name}");

                    var args = cn.EmitArgs(module, type, method, thenBlock, elseBlock);
                    currentValue = module.LlvmBuilder.BuildCall2(newMethod.Type, newMethod.LlvmMethod, args).ToMateriskValue();
                    break;
                }
                case CallNode: throw new Exception("Tried to call a non identifier in dot node stack.");
                default: throw new Exception("Unexpected node in dot node stack!");
            }

        return currentValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _callNode;
        foreach (var node in NextNodes) yield return node;
    }
}
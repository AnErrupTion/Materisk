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
                    if (currentValue is MateriskModule mModule)
                    {
                        var name = rvn.Name;

                        MateriskType? mType = null;

                        foreach (var typeDef in mModule.Types)
                            if (typeDef.Name == name)
                            {
                                mType = typeDef;
                                break;
                            }

                        currentValue = mType ?? throw new InvalidOperationException($"Unable to find type with name \"{name}\" in module: {mModule.Name}");
                    }
                    else
                    {
                        var name = rvn.Name;
                        var typeName = currentValue switch
                        {
                            MateriskMethodArgument argument => argument.TypeName,
                            MateriskLocalVariable variable => variable.TypeName,
                            MateriskType mType => mType.Name,
                            _ => method.ParentType.Name
                        };

                        var mFieldIndex = 0U;
                        MateriskField? mField = null;

                        foreach (var typeDef in module.Types)
                            foreach (var field in typeDef.Fields)
                            {
                                if (typeDef.Name == typeName && field.Name == name)
                                {
                                    mField = field;
                                    break;
                                }
                                mFieldIndex++;
                            }

                        if (mField is null)
                            throw new InvalidOperationException($"Unable to find field with name \"{name}\" in type: {module.Name}.{typeName}");

                        if (currentValue is MateriskMethodArgument or MateriskLocalVariable)
                            currentValue = mField.LoadInstance(currentValue.Load(), mFieldIndex).ToMateriskValue();
                        else
                            currentValue = mField;
                    }
                    break;
                }
                case AssignExpressionNode aen:
                {
                    if (currentValue is MateriskModule mModule)
                    {
                        var name = aen.Identifier;

                        MateriskType? mType = null;

                        foreach (var typeDef in mModule.Types)
                            if (typeDef.Name == name)
                            {
                                mType = typeDef;
                                break;
                            }

                        currentValue = mType ?? throw new InvalidOperationException($"Unable to find type with name \"{name}\" in module: {mModule.Name}");
                    }
                    else
                    {
                        var name = aen.Identifier;
                        var typeName = currentValue switch
                        {
                            MateriskMethodArgument argument => argument.TypeName,
                            MateriskLocalVariable variable => variable.TypeName,
                            MateriskType mType => mType.Name,
                            _ => method.ParentType.Name
                        };
                        var value = aen.Expr.Emit(module, type, method, thenBlock, elseBlock);

                        var mFieldIndex = 0U;
                        MateriskField? mField = null;

                        foreach (var typeDef in module.Types)
                            foreach (var field in typeDef.Fields)
                            {
                                if (typeDef.Name == typeName && field.Name == name)
                                {
                                    mField = field;
                                    break;
                                }
                                mFieldIndex++;
                            }

                        if (mField is null)
                            throw new InvalidOperationException($"Unable to find field with name \"{name}\" in module: {module.Name}");

                        if (currentValue is MateriskMethodArgument or MateriskLocalVariable)
                            mField.StoreInstance(currentValue.Load(), mFieldIndex, value.Load());
                        else
                            mField.Store(value.Load());
                    }
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
                    currentValue = module.LlvmBuilder.BuildCall2(newMethod.Type, newMethod.Load(), args).ToMateriskValue();
                    break;
                }
                case CallNode: throw new InvalidOperationException("Tried to call a non identifier in dot node stack.");
                default: throw new InvalidOperationException("Unexpected node in dot node stack!");
            }

        return currentValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _callNode;
        foreach (var node in NextNodes) yield return node;
    }
}
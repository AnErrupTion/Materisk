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
                        {
                            if (typeDef.Name != name)
                                continue;

                            if (typeDef.Attributes.HasFlag(MateriskAttributes.Struct))
                                throw new InvalidOperationException($"Trying to use the dot syntax on a struct: {typeDef.ParentModule.Name}.{typeDef.Name}");

                            mType = typeDef;
                            break;
                        }

                        currentValue = mType ?? throw new InvalidOperationException($"Unable to find type with name \"{name}\" in module: {mModule.Name}");
                    }
                    else
                    {
                        var typeName = currentValue switch
                        {
                            MateriskMethodArgument argument => argument.TypeName,
                            MateriskLocalVariable variable => variable.TypeName,
                            MateriskType mType => mType.Name,
                            _ => method.ParentType.Name
                        };
                        var name = rvn.Name;

                        var mFieldIndex = 0;
                        MateriskField? mField = null;

                        foreach (var typeDef in module.Types)
                        {
                            if (typeDef.Name == typeName && typeDef.Attributes.HasFlag(MateriskAttributes.Struct))
                                throw new InvalidOperationException($"Trying to use the dot syntax on a struct: {typeDef.ParentModule.Name}.{typeDef.Name}");

                            for (var i = 0; i < typeDef.Fields.Count; i++)
                            {
                                var field = typeDef.Fields[i];

                                if (typeDef.Name != typeName || field.Name != name)
                                    continue;

                                mField = field;
                                mFieldIndex = i;
                                break;
                            }
                        }

                        if (mField is null)
                            throw new InvalidOperationException($"Unable to find field with name \"{name}\" in type: {module.Name}.{typeName}");

                        if (currentValue is MateriskMethodArgument or MateriskLocalVariable)
                            currentValue = mField.LoadInstance(currentValue.Load(), (uint)mFieldIndex).ToMateriskValue();
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
                        {
                            if (typeDef.Name != name)
                                continue;

                            if (typeDef.Attributes.HasFlag(MateriskAttributes.Struct))
                                throw new InvalidOperationException($"Trying to use the dot syntax on a struct: {typeDef.ParentModule.Name}.{typeDef.Name}");

                            mType = typeDef;
                            break;
                        }

                        currentValue = mType ?? throw new InvalidOperationException($"Unable to find type with name \"{name}\" in module: {mModule.Name}");
                    }
                    else
                    {
                        var typeName = currentValue switch
                        {
                            MateriskMethodArgument argument => argument.TypeName,
                            MateriskLocalVariable variable => variable.TypeName,
                            MateriskType mType => mType.Name,
                            _ => method.ParentType.Name
                        };
                        var name = aen.Identifier;
                        var value = aen.Expr.Emit(module, type, method, thenBlock, elseBlock);

                        var mFieldIndex = 0;
                        MateriskField? mField = null;

                        foreach (var typeDef in module.Types)
                        {
                            if (typeDef.Name == typeName && typeDef.Attributes.HasFlag(MateriskAttributes.Struct))
                                throw new InvalidOperationException($"Trying to use the dot syntax on a struct: {typeDef.ParentModule.Name}.{typeDef.Name}");

                            for (var i = 0; i < typeDef.Fields.Count; i++)
                            {
                                var field = typeDef.Fields[i];

                                if (typeDef.Name != typeName || field.Name != name)
                                    continue;

                                mField = field;
                                mFieldIndex = i;
                                break;
                            }
                        }

                        if (mField is null)
                            throw new InvalidOperationException($"Unable to find field with name \"{name}\" in module: {module.Name}");

                        if (currentValue is MateriskMethodArgument or MateriskLocalVariable)
                            mField.StoreInstance(currentValue.Load(), (uint)mFieldIndex, value.Load());
                        else
                            mField.Store(value.Load());
                    }
                    break;
                }
                case CallNode { ToCallNode: IdentifierNode cnIdentNode } cn:
                {
                    if (currentValue is MateriskType newType && newType.Attributes.HasFlag(MateriskAttributes.Struct))
                        throw new InvalidOperationException($"Trying to use call a method on a struct: {newType.ParentModule.Name}.{newType.Name}");

                    var typeName = currentValue switch
                    {
                        MateriskMethodArgument argument => argument.TypeName,
                        MateriskLocalVariable variable => variable.TypeName,
                        MateriskType mType => mType.Name,
                        _ => method.ParentType.Name
                    };
                    var name = cnIdentNode.Name;
                    var (_, newMethod) = MateriskHelpers.GetOrCreateMethod(module, typeName!, name);

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
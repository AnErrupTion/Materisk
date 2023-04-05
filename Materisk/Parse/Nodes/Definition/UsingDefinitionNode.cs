﻿using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Definition;

internal class UsingDefinitionNode : SyntaxNode
{
    public readonly string Path;
    public readonly SyntaxNode RootNode;

    private MateriskType _lastType;

    public UsingDefinitionNode(string path, SyntaxNode rootNode)
    {
        Path = path;
        RootNode = rootNode;
    }

    public override NodeType Type => NodeType.UsingDefinition;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        WalkTree(RootNode, module);

        var moduleName = System.IO.Path.GetFileNameWithoutExtension(Path);
        MateriskHelpers.CreateModuleEmit(moduleName, RootNode);

        return null!;
    }
    
    private void WalkTree(SyntaxNode node, MateriskModule module)
    {
        switch (node)
        {
            case ModuleDefinitionNode modNode:
            {
                var newType = new MateriskType(
                    module,
                    modNode.Name,
                    MateriskAttributesUtils.CreateAttributes(modNode.IsPublic, false, false, false));
                _lastType = newType;
                module.Types.Add(newType);
                break;
            }
            case ModuleFunctionDefinitionNode modFuncNode:
            {
                var newMethod = MateriskHelpers.AddMethod(
                    module,
                    _lastType,
                    modFuncNode.Name,
                    modFuncNode.Args,
                    modFuncNode.IsPublic,
                    modFuncNode.IsStatic,
                    modFuncNode.IsNative,
                    true,
                    modFuncNode.ReturnType,
                    modFuncNode.SecondReturnType);
                _lastType.Methods.Add(newMethod);
                break;
            }
            case FunctionDefinitionNode funcNode:
            {
                var mType = module.Types[0];
                var newMethod = MateriskHelpers.AddMethod(
                    module,
                    mType,
                    funcNode.Name,
                    funcNode.Args,
                    funcNode.IsPublic,
                    true,
                    funcNode.IsNative,
                    true,
                    funcNode.ReturnType,
                    funcNode.SecondReturnType);
                module.Types[0].Methods.Add(newMethod);
                break;
            }
        }

        foreach (var n in node.GetChildren())
            WalkTree(n, module);
    }
}
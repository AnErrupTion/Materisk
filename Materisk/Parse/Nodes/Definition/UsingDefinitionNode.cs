using LLVMSharp.Interop;
using Materisk.TypeSystem;

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

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        WalkTree(RootNode, module);
        return MateriskHelpers.CreateModuleEmit(System.IO.Path.GetFileNameWithoutExtension(Path), RootNode);
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
            case FunctionDefinitionNode funcNode:
            {
                var newMethod = MateriskHelpers.AddMethod(
                    module,
                    _lastType,
                    funcNode.Name,
                    funcNode.Args,
                    funcNode.IsPublic,
                    funcNode.IsStatic,
                    funcNode.IsNative,
                    true,
                    funcNode.ReturnType,
                    funcNode.SecondReturnType);
                _lastType.Methods.Add(newMethod);
                break;
            }
        }

        foreach (var n in node.GetChildren())
            WalkTree(n, module);
    }
}
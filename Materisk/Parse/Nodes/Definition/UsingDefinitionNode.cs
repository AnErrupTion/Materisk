using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Definition;

internal class UsingDefinitionNode : SyntaxNode
{
    private readonly string _identifier;
    private readonly SyntaxNode _rootNode;
    private MateriskType? _lastType;

    public UsingDefinitionNode(string identifier, SyntaxNode rootNode)
    {
        _identifier = identifier;
        _rootNode = rootNode;
        _lastType = null;
    }

    public override NodeType Type => NodeType.UsingDefinition;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        WalkTree(_rootNode, module);

        var newModule = MateriskHelpers.CreateModuleEmit(_identifier, _rootNode);

        module.Imports.Add(_identifier, newModule);

        return newModule;
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
                    _lastType!,
                    funcNode.Name,
                    funcNode.Args,
                    funcNode.IsPublic,
                    funcNode.IsStatic,
                    funcNode.IsNative,
                    true,
                    funcNode.ReturnType,
                    funcNode.SecondReturnType);
                _lastType!.Methods.Add(newMethod);
                break;
            }
        }

        foreach (var n in node.GetChildren())
            WalkTree(n, module);
    }
}
using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Definition;

internal class UsingDefinitionNode : SyntaxNode
{
    private readonly string _identifier;
    private readonly SyntaxNode _rootNode;

    public UsingDefinitionNode(string identifier, SyntaxNode rootNode)
    {
        _identifier = identifier;
        _rootNode = rootNode;
    }

    public override NodeType Type => NodeType.UsingDefinition;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var newModule = MateriskHelpers.CreateModuleEmit(_identifier, _rootNode);

        module.Imports.Add(_identifier, newModule);

        return newModule;
    }
}
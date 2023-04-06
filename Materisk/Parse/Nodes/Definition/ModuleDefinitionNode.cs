using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Definition;

internal class ModuleDefinitionNode : SyntaxNode
{
    public readonly string Name;
    public readonly IEnumerable<SyntaxNode> Body;
    public readonly bool IsPublic;
    public readonly bool IsStatic;

    public ModuleDefinitionNode(string name, IEnumerable<SyntaxNode> body, bool isPublic, bool isStatic)
    {
        Name = name;
        Body = body;
        IsPublic = isPublic;
        IsStatic = isStatic;
    }

    public override NodeType Type => NodeType.ModuleDefinition;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        var newType = new MateriskType(module, Name, MateriskAttributesUtils.CreateAttributes(IsPublic, IsStatic, false, false));

        module.Types.Add(newType);
        
        foreach (var node in Body)
            if (node is FieldDefinitionNode)
                node.Emit(module, newType, method, thenBlock, elseBlock);
        
        newType.BuildStruct();

        foreach (var node in Body)
            if (node is not FieldDefinitionNode)
                node.Emit(module, newType, method, thenBlock, elseBlock);

        return newType;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Body;
    }
}
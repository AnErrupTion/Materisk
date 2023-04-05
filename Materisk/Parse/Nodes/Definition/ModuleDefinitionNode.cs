using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Definition;

internal class ModuleDefinitionNode : SyntaxNode
{
    public readonly string Name;
    public readonly IEnumerable<SyntaxNode> Body;
    public readonly bool IsPublic;

    public ModuleDefinitionNode(string name, IEnumerable<SyntaxNode> body, bool isPublic)
    {
        Name = name;
        Body = body;
        IsPublic = isPublic;
    }

    public override NodeType Type => NodeType.ModuleDefinition;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var newType = new MateriskType(module, Name, MateriskAttributesUtils.CreateAttributes(IsPublic, false, false, false));

        module.Types.Add(newType);

        foreach (var bodyNode in Body)
            bodyNode.Emit(module, newType, method, metadata);

        return newType;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Body;
    }
}
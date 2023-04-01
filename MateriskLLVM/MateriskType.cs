namespace MateriskLLVM;

public sealed class MateriskType
{
    public readonly MateriskModule ParentModule;
    public readonly string Name;
    public readonly List<MateriskField> Fields;
    public readonly List<MateriskMethod> Methods;

    public MateriskType(MateriskModule module, string name)
    {
        ParentModule = module;
        Name = name;
        Fields = new();
        Methods = new();
    }
}
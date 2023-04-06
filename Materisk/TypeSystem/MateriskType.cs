using LLVMSharp.Interop;

namespace Materisk.TypeSystem;

public sealed class MateriskType : MateriskUnit
{
    public readonly MateriskModule ParentModule;
    public readonly string Name;
    public readonly List<MateriskField> Fields;
    public readonly List<MateriskMethod> Methods;

    public MateriskType(MateriskModule module, string name, MateriskAttributes attributes)
    {
        ParentModule = module;
        Name = name;
        Attributes = attributes;
        Fields = new();
        Methods = new();
    }

    public override LLVMValueRef Load() => throw new NotSupportedException();

    public override LLVMValueRef Store(LLVMValueRef value) => throw new NotSupportedException();
}
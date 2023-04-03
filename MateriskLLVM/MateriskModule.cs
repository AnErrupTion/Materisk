using LLVMSharp.Interop;

namespace MateriskLLVM;

public sealed class MateriskModule : MateriskUnit
{
    public LLVMModuleRef LlvmModule;
    public LLVMBuilderRef LlvmBuilder;

    public readonly string Name;
    public readonly List<MateriskType> Types;

    public MateriskModule(string name)
    {
        LlvmModule = LLVMModuleRef.CreateWithName(name);
        LlvmBuilder = LLVMBuilderRef.Create(LLVMContextRef.Global);

        Name = name;
        Types = new();
    }

    public override LLVMValueRef Load() => throw new NotImplementedException();

    public override LLVMValueRef Store(LLVMValueRef value) => throw new NotImplementedException();
}
using LLVMSharp.Interop;
using Materisk.Utils;

namespace Materisk.TypeSystem;

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

        unsafe { LLVM.SetModuleDataLayout(LlvmModule, LlvmUtils.DataLayout); }
        LlvmModule.Target = LlvmUtils.TargetTriple;

        Name = name;
        Types = new();
    }

    public override LLVMValueRef Load() => throw new NotSupportedException();

    public override LLVMValueRef Store(LLVMValueRef value) => throw new NotSupportedException();
}
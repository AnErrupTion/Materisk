using LLVMSharp.Interop;
using Materisk.Utils;

namespace Materisk.TypeSystem;

public sealed class MateriskModule : MateriskUnit
{
    public LLVMModuleRef LlvmModule;
    public LLVMBuilderRef LlvmBuilder;

    public uint Counter;

    public readonly string Name;
    public readonly List<MateriskType> Types;
    public readonly Dictionary<string, MateriskModule> Imports;

    public MateriskModule(string name)
    {
        LlvmModule = LLVMModuleRef.CreateWithName(name);
        LlvmBuilder = LLVMBuilderRef.Create(LLVMContextRef.Global);

        Counter = 0;

        unsafe { LLVM.SetModuleDataLayout(LlvmModule, LlvmUtils.DataLayout); }
        LlvmModule.Target = LlvmUtils.TargetTriple;

        Name = name;
        Types = new();
        Imports = new();
    }

    public override LLVMValueRef Load() => throw new NotSupportedException();

    public override LLVMValueRef Store(LLVMValueRef value) => throw new NotSupportedException();
}
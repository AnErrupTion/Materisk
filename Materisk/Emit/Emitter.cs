using System.Diagnostics;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Parse.Nodes;

namespace Materisk.Emit;

public class Emitter
{
    private readonly string _moduleName;
    private readonly SyntaxNode _rootNode;

    public Emitter(string moduleName, SyntaxNode rootNode)
    {
        _moduleName = moduleName;
        _rootNode = rootNode;
    }

    public void Emit(bool noStdLib, string targetTriple, string cpu, string features)
    {
        if (noStdLib)
            LlvmUtils.MainFunctionNameOverride = "_start";
        
        LLVM.InitializeAllTargetInfos();
        LLVM.InitializeAllTargets();
        LLVM.InitializeAllTargetMCs();
        LLVM.InitializeAllAsmParsers();
        LLVM.InitializeAllAsmPrinters();

        var target = LLVMTargetRef.GetTargetFromTriple(targetTriple);
        var targetMachine = target.CreateTargetMachine(
            targetTriple,
            cpu, features,
            LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
            LLVMRelocMode.LLVMRelocDefault,
            LLVMCodeModel.LLVMCodeModelDefault);
        var dataLayout = targetMachine.CreateTargetDataLayout();

        var module = new MateriskModule(_moduleName);
        unsafe { LLVM.SetModuleDataLayout(module.LlvmModule, dataLayout); }
        module.LlvmModule.Target = targetTriple;

        var type = new MateriskType(module, "Program");
        module.Types.Add(type);

        _rootNode.Emit(module, type, null!);

        module.LlvmModule.Dump();
        module.LlvmModule.Verify(LLVMVerifierFailureAction.LLVMAbortProcessAction);

        var path = $"{_moduleName}.o";
        targetMachine.EmitToFile(module.LlvmModule, path, LLVMCodeGenFileType.LLVMObjectFile);
        Process.Start("clang", $"{path} -o {_moduleName}{(noStdLib ? " -nostdlib" : string.Empty)}").WaitForExit();
    }
}
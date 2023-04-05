using System.Diagnostics;
using System.Text;
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

        if (!Directory.Exists("output"))
            Directory.CreateDirectory("output");

        LLVM.InitializeAllTargetInfos();
        LLVM.InitializeAllTargets();
        LLVM.InitializeAllTargetMCs();
        LLVM.InitializeAllAsmParsers();
        LLVM.InitializeAllAsmPrinters();

        var target = LLVMTargetRef.GetTargetFromTriple(targetTriple);
        LlvmUtils.TargetTriple = targetTriple;
        LlvmUtils.TargetMachine = target.CreateTargetMachine(
            targetTriple,
            cpu, features,
            LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
            LLVMRelocMode.LLVMRelocDefault,
            LLVMCodeModel.LLVMCodeModelDefault);
        LlvmUtils.DataLayout = LlvmUtils.TargetMachine.CreateTargetDataLayout();

        var module = new MateriskModule(_moduleName);
        unsafe { LLVM.SetModuleDataLayout(module.LlvmModule, LlvmUtils.DataLayout); }
        module.LlvmModule.Target = targetTriple;

        var type = new MateriskType(
            module,
            "Program",
            MateriskAttributesUtils.CreateAttributes(true, false, false, false));
        module.Types.Add(type);

        var metadata = new MateriskMetadata();
        _rootNode.Emit(module, type, null!, metadata);

        module.LlvmModule.Dump();
        module.LlvmModule.Verify(LLVMVerifierFailureAction.LLVMAbortProcessAction);

        LlvmUtils.TargetMachine.EmitToFile(
            module.LlvmModule,
            $"output/{_moduleName}.o",
            LLVMCodeGenFileType.LLVMObjectFile);

        var args = new StringBuilder();
        foreach (var file in Directory.GetFiles("output"))
            args.Append(file).Append(' ');
        args.Append("-o ").Append(_moduleName);
        if (noStdLib)
            args.Append(" -nostdlib");

        Process.Start("clang", args.ToString()).WaitForExit();
    }
}
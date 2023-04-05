using LLVMSharp.Interop;
using Materisk.TypeSystem;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class UsingDefinitionNode : SyntaxNode
{
    public readonly string Path;
    public readonly SyntaxNode RootNode;

    private MateriskType _lastType;

    public UsingDefinitionNode(string path, SyntaxNode rootNode)
    {
        Path = path;
        RootNode = rootNode;
    }

    public override NodeType Type => NodeType.UsingDefinition;

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        WalkTree(RootNode, module);

        var moduleName = System.IO.Path.GetFileNameWithoutExtension(Path);

        var newModule = new MateriskModule(moduleName);
        unsafe { LLVM.SetModuleDataLayout(newModule.LlvmModule, LlvmUtils.DataLayout); }
        newModule.LlvmModule.Target = LlvmUtils.TargetTriple;

        var newType = new MateriskType(newModule, "Program", MateriskAttributesUtils.CreateAttributes(true, false, false, false));
        newModule.Types.Add(newType);

        var newMetadata = new MateriskMetadata();
        RootNode.Emit(newModule, newType, null!, newMetadata);

        newModule.LlvmModule.Dump();
        newModule.LlvmModule.Verify(LLVMVerifierFailureAction.LLVMAbortProcessAction);

        LlvmUtils.TargetMachine.EmitToFile(newModule.LlvmModule, $"output/{moduleName}.o", LLVMCodeGenFileType.LLVMObjectFile);
        return null!;
    }
    
    private void WalkTree(SyntaxNode node, MateriskModule module)
    {
        switch (node)
        {
            case ModuleDefinitionNode modNode: AddModule(modNode, module); break;
            case ModuleFunctionDefinitionNode modFuncNode: AddModuleMethod(modFuncNode, module); break;
            case FunctionDefinitionNode funcNode: AddMethod(funcNode, module); break;
        }

        foreach (var n in node.GetChildren())
            WalkTree(n, module);
    }

    private void AddModule(ModuleDefinitionNode node, MateriskModule module)
    {
        var newType = new MateriskType(module, node.Name, MateriskAttributesUtils.CreateAttributes(node.IsPublic, false, false, true));
        _lastType = newType;
        module.Types.Add(newType);
    }

    private void AddModuleMethod(ModuleFunctionDefinitionNode node, MateriskModule module)
    {
        var argts = new List<MateriskMethodArgument>();
        var parameters = new List<LLVMTypeRef>();

        foreach (var arg in node.Args)
        {
            var firstType = arg.Type;
            var secondType = arg.SecondType;
            var argType = TypeSigUtils.GetTypeSignatureFor(module, firstType, secondType);
            var pointerElementType = firstType switch
            {
                "ptr" or "arr" when !string.IsNullOrEmpty(secondType) => TypeSigUtils.GetTypeSignatureFor(module, secondType),
                "str" => LLVMTypeRef.Int8,
                _ => null
            };

            parameters.Add(argType);
            argts.Add(new(arg.Name, argType, pointerElementType, firstType[0] is 'i'));
        }

        var newMethod = new MateriskMethod(
            _lastType,
            node.Name,
            MateriskAttributesUtils.CreateAttributes(node.IsPublic, node.IsStatic, node.IsNative, true),
            LLVMTypeRef.CreateFunction(
                TypeSigUtils.GetTypeSignatureFor(module, node.ReturnType, node.SecondReturnType),
                parameters.ToArray()),
            argts.ToArray());

        _lastType.Methods.Add(newMethod);
    }

    private static void AddMethod(FunctionDefinitionNode node, MateriskModule module)
    {
        var argts = new List<MateriskMethodArgument>();
        var parameters = new List<LLVMTypeRef>();

        foreach (var arg in node.Args)
        {
            var firstType = arg.Type;
            var secondType = arg.SecondType;
            var argType = TypeSigUtils.GetTypeSignatureFor(module, firstType, secondType);
            var pointerElementType = firstType switch
            {
                "ptr" or "arr" when !string.IsNullOrEmpty(secondType) => TypeSigUtils.GetTypeSignatureFor(module, secondType),
                "str" => LLVMTypeRef.Int8,
                _ => null
            };

            parameters.Add(argType);
            argts.Add(new(arg.Name, argType, pointerElementType, firstType[0] is 'i'));
        }

        var newMethod = new MateriskMethod(
            module.Types[0],
            node.Name,
            MateriskAttributesUtils.CreateAttributes(node.IsPublic, true, node.IsNative, true),
            LLVMTypeRef.CreateFunction(
                TypeSigUtils.GetTypeSignatureFor(module, node.ReturnType, node.SecondReturnType),
                parameters.ToArray()),
            argts.ToArray());

        module.Types[0].Methods.Add(newMethod);
    }
}
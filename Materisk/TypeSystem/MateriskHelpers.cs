using LLVMSharp.Interop;
using Materisk.Parse;
using Materisk.Parse.Nodes;
using Materisk.Utils;

namespace Materisk.TypeSystem;

internal static class MateriskHelpers
{
    public static void CreateModuleEmit(string name, SyntaxNode rootNode)
    {
        if (!Directory.Exists("output")) Directory.CreateDirectory("output");
        if (!Directory.Exists("llvm")) Directory.CreateDirectory("llvm");

        var module = new MateriskModule(name);
        var type = new MateriskType(
            module,
            name,
            MateriskAttributesUtils.CreateAttributes(true, false, false, false));
        module.Types.Add(type);

        var metadata = new MateriskMetadata();
        rootNode.Emit(module, type, null!, metadata);

        module.LlvmModule.WriteBitcodeToFile($"llvm/{name}.ir");
        module.LlvmModule.Verify(LLVMVerifierFailureAction.LLVMAbortProcessAction);

        LlvmUtils.TargetMachine.EmitToFile(module.LlvmModule, $"output/{name}.o", LLVMCodeGenFileType.LLVMObjectFile);
    }

    public static MateriskMethod AddMethod(MateriskModule module, MateriskType type, string name, List<MethodArgument> args, bool isPublic, bool isStatic, bool isNative, bool isExternal, string returnType, string secondReturnType)
    {
        var argts = new List<MateriskMethodArgument>();
        var parameters = new List<LLVMTypeRef>();

        foreach (var arg in args)
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
            type,
            name,
            MateriskAttributesUtils.CreateAttributes(isPublic, isStatic, isNative, isExternal),
            LLVMTypeRef.CreateFunction(
                TypeSigUtils.GetTypeSignatureFor(module, returnType, secondReturnType),
                parameters.ToArray()),
            argts.ToArray());

        return newMethod;
    }
}
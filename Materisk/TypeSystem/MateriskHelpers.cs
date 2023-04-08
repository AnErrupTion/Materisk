using LLVMSharp.Interop;
using Materisk.Parse;
using Materisk.Parse.Nodes;
using Materisk.Utils;

namespace Materisk.TypeSystem;

internal static class MateriskHelpers
{
    public static MateriskValue ToMateriskValue(this LLVMValueRef value)
    {
        return new(value);
    }

    public static MateriskModule CreateModuleEmit(string name, SyntaxNode rootNode)
    {
        if (!Directory.Exists("output")) Directory.CreateDirectory("output");
        if (!Directory.Exists("llvm")) Directory.CreateDirectory("llvm");

        var module = new MateriskModule(name);

        rootNode.Emit(module, null!, null!, null!, null!);

        module.LlvmModule.WriteBitcodeToFile($"llvm/{name}.ir");
        module.LlvmModule.Dump();
        module.LlvmModule.Verify(LLVMVerifierFailureAction.LLVMAbortProcessAction);

        LlvmUtils.TargetMachine.EmitToFile(module.LlvmModule, $"output/{name}.o", LLVMCodeGenFileType.LLVMObjectFile);
        return module;
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

            string typeName;
            LLVMTypeRef pointerElementType = null;

            switch (firstType)
            {
                case "ptr" or "arr" when !string.IsNullOrEmpty(secondType):
                {
                    typeName = secondType;
                    pointerElementType = TypeSigUtils.GetTypeSignatureFor(module, secondType);
                    break;
                }
                case "str":
                {
                    typeName = "str";
                    pointerElementType = LLVMTypeRef.Int8;
                    break;
                }
                default:
                {
                    typeName = firstType;
                    break;
                }
            }

            parameters.Add(argType);
            argts.Add(new(arg.Name, typeName, argType, pointerElementType, firstType[0] is 'i'));
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
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
        //if (!Directory.Exists("llvm")) Directory.CreateDirectory("llvm");

        var module = new MateriskModule(name);

        rootNode.Emit(module, null!, null!, null!, null!);

        //module.LlvmModule.WriteBitcodeToFile($"llvm/{name}.ir");
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
            string firstType;
            bool firstTypeIsPointer;
            string secondType;

            if (arg.Name is "self" && !isStatic)
            {
                firstType = "ptr";
                firstTypeIsPointer = false; // In the Materisk code, it's "X self", not "ptr X self"!
                secondType = arg.Type;
            }
            else
            {
                firstType = arg.Type;
                firstTypeIsPointer = firstType is "ptr";
                secondType = arg.SecondType;
            }

            var argType = TypeSigUtils.GetTypeSignatureFor(module, firstType, firstTypeIsPointer, secondType);

            string typeName;
            LLVMTypeRef pointerElementType = null;

            switch (firstType)
            {
                case "ptr" or "arr" when !string.IsNullOrEmpty(secondType):
                {
                    typeName = secondType;
                    pointerElementType = TypeSigUtils.GetTypeSignatureFor(module, secondType, firstTypeIsPointer);
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
            MateriskAttributesUtils.CreateAttributes(isPublic, isStatic, isNative, isExternal, false),
            LLVMTypeRef.CreateFunction(
                TypeSigUtils.GetTypeSignatureFor(module, returnType, returnType is "ptr", secondReturnType),
                parameters.ToArray()),
            argts.ToArray());

        return newMethod;
    }

    public static (MateriskType, MateriskMethod) GetOrCreateMethod(MateriskModule module, string typeName, string name, bool isExternal = true)
    {
        MateriskType? newType = null;
        MateriskMethod? newMethod = null;

        foreach (var typeDef in module.Types)
        {
            if (typeDef.Name == typeName)
                newType = typeDef;

            foreach (var meth in typeDef.Methods)
            {
                if (newType is null || meth.Name != name)
                    continue;

                newMethod = meth;
                break;
            }
        }

        if (newType is null)
        {
            MateriskType? resolvedType = null;

            foreach (var import in module.Imports)
            {
                foreach (var typeDef in import.Value.Types)
                {
                    if (typeDef.Name == typeName)
                    {
                        resolvedType = typeDef;
                        break;
                    }
                }
            }
            
            if (resolvedType is null)
                throw new InvalidOperationException($"Unable to find type with name: {typeName}");

            newType = new MateriskType(
                module,
                resolvedType.Name,
                resolvedType.Attributes
            );
            module.Types.Add(newType);
        }

        if (newMethod is null)
        {
            MateriskMethod? resolvedMethod = null;

            foreach (var import in module.Imports)
            {
                foreach (var typeDef in import.Value.Types)
                {
                    foreach (var meth in typeDef.Methods)
                    {
                        if (typeDef.Name != typeName || meth.Name != name)
                            continue;

                        resolvedMethod = meth;
                        break;
                    }
                }
            }

            if (resolvedMethod is null)
                throw new InvalidOperationException($"Unable to find method with name: {typeName}.{name}");

            var attributes = resolvedMethod.Attributes;

            if (isExternal)
                attributes |= MateriskAttributes.External;
            else if (attributes.HasFlag(MateriskAttributes.External))
                attributes &= ~MateriskAttributes.External;

            newMethod = new MateriskMethod(
                newType,
                name,
                attributes,
                resolvedMethod.Type,
                resolvedMethod.Arguments
            );
            newType.Methods.Add(newMethod);
        }

        return (newType, newMethod);
    }
}
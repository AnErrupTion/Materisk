using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Native;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class ModuleFunctionDefinitionNode : SyntaxNode
{
    private readonly string _name;
    private readonly List<MethodArgument> _args;
    private readonly string _returnType;
    private readonly string _secondReturnType;
    private readonly SyntaxNode _body;
    private readonly bool _isStatic;
    private readonly bool _isPublic;
    private readonly bool _isNative;

    public ModuleFunctionDefinitionNode(string name, List<MethodArgument> args, string returnType, string secondReturnType, SyntaxNode body, bool isStatic, bool isPublic, bool isNative)
    {
        _name = name;
        _args = args;
        _returnType = returnType;
        _secondReturnType = secondReturnType;
        _body = body;
        _isStatic = isStatic;
        _isPublic = isPublic;
        _isNative = isNative;
    }

    public override NodeType Type => NodeType.ModuleFunctionDefinition;

    // TODO: Constructor and instance methods
    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var argts = new List<MateriskMethodArgument>();
        var parameters = new List<LLVMTypeRef>();

        foreach (var arg in _args)
        {
            var firstType = arg.Type;
            var secondType = arg.SecondType;
            var argType = TypeSigUtils.GetTypeSignatureFor(firstType, secondType);
            var pointerElementType = firstType switch
            {
                "ptr" or "arr" when !string.IsNullOrEmpty(secondType) => TypeSigUtils.GetTypeSignatureFor(secondType),
                "str" => LLVMTypeRef.Int8,
                _ => null
            };

            parameters.Add(argType);
            argts.Add(new(arg.Name, argType, pointerElementType, firstType[0] is 'i'));
        }

        var newMethod = new MateriskMethod(
            type,
            _name,
            MateriskAttributesUtils.CreateAttributes(_isPublic, _isStatic, _isNative),
            LLVMTypeRef.CreateFunction(
                TypeSigUtils.GetTypeSignatureFor(_returnType, _secondReturnType),
                parameters.ToArray()),
            argts.ToArray());

        type.Methods.Add(newMethod);

        if (!_isNative)
            _body.Emit(module, type, newMethod, metadata);
        else
            LlvmNativeFuncImpl.Emit(module, type.Name, newMethod);

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _body;
    }
}
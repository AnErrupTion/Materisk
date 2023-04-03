using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Native;
using Materisk.Parse.Nodes.Misc;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class FunctionDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken _nameToken;
    private readonly Dictionary<Tuple<SyntaxToken, SyntaxToken?>, SyntaxToken> _args;
    private readonly SyntaxToken _returnType;
    private readonly SyntaxToken? _secondReturnType;
    private readonly SyntaxNode _block;
    private readonly bool _isPublic;
    private readonly bool _isNative;

    public FunctionDefinitionNode(SyntaxToken nameToken, Dictionary<Tuple<SyntaxToken, SyntaxToken?>, SyntaxToken> args, SyntaxToken returnType, SyntaxToken? secondReturnType, SyntaxNode block, bool isPublic, bool isNative)
    {
        _nameToken = nameToken;
        _args = args;
        _returnType = returnType;
        _secondReturnType = secondReturnType;
        _block = block;
        _isPublic = isPublic;
        _isNative = isNative;
    }

    public override NodeType Type => NodeType.FunctionDefinition;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var argts = new List<MateriskMethodArgument>();
        var parameters = new List<LLVMTypeRef>();

        foreach (var arg in _args)
        {
            var firstType = arg.Key.Item1.Text;
            var secondType = arg.Key.Item2?.Text;
            var argType = TypeSigUtils.GetTypeSignatureFor(firstType, secondType);
            var pointerElementType = firstType switch
            {
                "ptr" or "arr" when secondType is not null => TypeSigUtils.GetTypeSignatureFor(secondType),
                "str" => LLVMTypeRef.Int8,
                _ => null
            };

            parameters.Add(argType);
            argts.Add(new(arg.Value.Text, argType, pointerElementType, firstType[0] is 'i'));
        }

        var newMethod = new MateriskMethod(
            module.Types[0],
            _nameToken.Text,
            LLVMTypeRef.CreateFunction(
                TypeSigUtils.GetTypeSignatureFor(_returnType.Text, _secondReturnType?.Text),
                parameters.ToArray()),
            argts.ToArray());

        module.Types[0].Methods.Add(newMethod);

        var mType = module.Types[0];

        if (!_isNative)
        {
            var lastValue = _block.Emit(module, mType, newMethod, metadata);
            if (lastValue is not null)
                module.LlvmBuilder.BuildRetVoid();
        } else LlvmNativeFuncImpl.Emit(module, mType.Name, newMethod);

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_nameToken);
        foreach (var t in _args) yield return new TokenNode(t.Value);
        yield return _block;
    }
}
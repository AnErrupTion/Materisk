using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.Lex;
using Materisk.Native;
using Materisk.Utils;

namespace Materisk.Parse.Nodes;

internal class FunctionDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken _nameToken;
    private readonly Dictionary<SyntaxToken, SyntaxToken> _args;
    private readonly SyntaxToken _returnType;
    private readonly SyntaxNode _block;
    private readonly bool _isPublic;
    private readonly bool _isNative;

    public FunctionDefinitionNode(SyntaxToken nameToken, Dictionary<SyntaxToken, SyntaxToken> args, SyntaxToken returnType, SyntaxNode block, bool isPublic, bool isNative)
    {
        _nameToken = nameToken;
        _args = args;
        _returnType = returnType;
        _block = block;
        _isPublic = isPublic;
        _isNative = isNative;
    }

    public override NodeType Type => NodeType.FunctionDefinition;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var attributes = MethodAttributes.Static;

        if (_isPublic)
            attributes |= MethodAttributes.Public;

        var parameters = new List<TypeSignature>();
        var argts = new List<string>();

        foreach (var arg in _args)
        {
            parameters.Add(TypeSigUtils.GetTypeSignatureFor(module, arg.Key.Text));
            argts.Add(arg.Value.Text);
        }

        var newMethod = new MethodDefinition(_nameToken.Text,
            attributes,
            MethodSignature.CreateStatic(TypeSigUtils.GetTypeSignatureFor(module, _returnType.Text), parameters));
        newMethod.CilMethodBody = new(newMethod);

        module.TopLevelTypes[1].Methods.Add(newMethod);

        var typeDef = module.TopLevelTypes[1];

        if (_isNative)
            CilNativeFuncImpl.Emit(module, typeDef.Name, newMethod);
        else
            _block.Emit(variables, module, typeDef, newMethod, argts);

        newMethod.CilMethodBody.Instructions.Add(CilOpCodes.Ret);

        if (newMethod.Name == "main")
            module.ManagedEntryPointMethod = newMethod;

        newMethod.CilMethodBody?.Instructions.CalculateOffsets();
        Console.WriteLine(newMethod.Name);
        foreach (var inst in newMethod.CilMethodBody.Instructions)
            Console.WriteLine(inst.ToString());
        Console.WriteLine("----");

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_nameToken);
        foreach (var t in _args) yield return new TokenNode(t.Value);
        yield return _block;
    }
}
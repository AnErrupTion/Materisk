using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.BuiltinTypes;
using Materisk.Lex;
using Materisk.Native;

namespace Materisk.Parse.Nodes;

internal class FunctionDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken nameToken;
    private readonly Dictionary<SyntaxToken, SyntaxToken> args;
    private readonly SyntaxToken returnType;
    private readonly SyntaxNode block;
    private readonly bool isPublic;
    private readonly bool isNative;

    public FunctionDefinitionNode(SyntaxToken nameToken, Dictionary<SyntaxToken, SyntaxToken> args, SyntaxToken returnType, SyntaxNode block, bool isPublic, bool isNative)
    {
        this.nameToken = nameToken;
        this.args = args;
        this.returnType = returnType;
        this.block = block;
        this.isPublic = isPublic;
        this.isNative = isNative;
    }

    public override NodeType Type => NodeType.FunctionDefinition;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var attributes = MethodAttributes.Static;

        if (isPublic)
            attributes |= MethodAttributes.Public;

        var parameters = new List<TypeSignature>();
        var argts = new List<string>();

        foreach (var arg in args)
        {
            parameters.Add(Utils.GetTypeSignatureFor(module, arg.Key.Text));
            argts.Add(arg.Value.Text);
        }

        var newMethod = new MethodDefinition(nameToken.Text,
            attributes,
            MethodSignature.CreateStatic(Utils.GetTypeSignatureFor(module, returnType.Text), parameters));
        newMethod.CilMethodBody = new(newMethod);

        module.TopLevelTypes[1].Methods.Add(newMethod);

        var typeDef = module.TopLevelTypes[1];

        if (isNative)
            CilNativeFuncImpl.Emit(module, typeDef.Name, newMethod);
        else
            block.Emit(variables, module, typeDef, newMethod, argts);

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
        yield return new TokenNode(nameToken);
        foreach (var t in args) yield return new TokenNode(t.Value);
        yield return block;
    }
}
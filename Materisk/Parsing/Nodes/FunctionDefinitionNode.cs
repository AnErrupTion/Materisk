using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.BuiltinTypes;
using Materisk.Native;

namespace Materisk.Parsing.Nodes;

internal class FunctionDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken? nameToken;
    private readonly Dictionary<SyntaxToken, SyntaxToken> args;
    private readonly SyntaxToken returnType;
    private readonly SyntaxNode block;
    private readonly bool isPublic;
    private readonly bool isNative;

    public FunctionDefinitionNode(SyntaxToken? nameToken, Dictionary<SyntaxToken, SyntaxToken> args, SyntaxToken returnType, SyntaxNode block, bool isPublic, bool isNative)
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
        SBaseFunction f;
        var name = nameToken?.Text ?? "<anonymous>";

        if (isNative)
        {
            f = new SNativeFunction(name, NativeFuncImpl.GetImplFor(name), args.Select(v => v.Value.Text).ToList())
            {
                IsPublic = isPublic
            };
        }
        else
        {
            f = new SFunction(scope, name, args.Select(v => v.Value.Text).ToList(), block)
            {
                IsPublic = isPublic
            };
        }

        if (name != "<anonymous>")
        {
            scope.Set(name, f);
            if (isPublic) scope.GetRoot().ExportTable.Add(name, f);
        }
        return f;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        if (nameToken is null)
            throw new InvalidOperationException("Function name is null.");

        var attributes = MethodAttributes.Static;

        if (isPublic)
            attributes |= MethodAttributes.Public;

        var parameters = new List<TypeSignature>();
        var argts = new List<string>();

        foreach (var arg in args)
        {
            parameters.Add(Utils.GetTypeSignatureFor(module, arg.Key.Value.ToString()));
            argts.Add(arg.Value.Text);
        }

        var newMethod = new MethodDefinition(nameToken.Value.Text,
            attributes,
            MethodSignature.CreateStatic(Utils.GetTypeSignatureFor(module, returnType.Value.ToString()), parameters));
        newMethod.CilMethodBody = new(newMethod);

        module.TopLevelTypes[1].Methods.Add(newMethod);

        if (isNative)
            CilNativeFuncImpl.Emit(module, module.TopLevelTypes[1].Name, newMethod);
        else
            block.Emit(variables, module, newMethod, argts);

        newMethod.CilMethodBody.Instructions.Add(CilOpCodes.Ret);

        foreach (var inst in newMethod.CilMethodBody.Instructions)
            Console.WriteLine(inst.ToString());
        Console.WriteLine("----");

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (nameToken != null) yield return new TokenNode(nameToken.Value);
        foreach (var t in args) yield return new TokenNode(t.Value);
        yield return block;
    }
}
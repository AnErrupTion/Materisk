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
    private readonly List<SyntaxToken> args;
    private readonly SyntaxNode block;
    private readonly bool isPublic;
    private readonly bool isNative;

    public FunctionDefinitionNode(SyntaxToken? nameToken, List<SyntaxToken> args, SyntaxNode block, bool isPublic, bool isNative)
    {
        this.nameToken = nameToken;
        this.args = args;
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
            f = new SNativeFunction(name, NativeFuncImpl.GetImplFor(name), args.Select(v => v.Text).ToList())
            {
                IsPublic = isPublic
            };
        }
        else
        {
            f = new SFunction(scope, name, args.Select(v => v.Text).ToList(), block)
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

    // TODO: Native functions
    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, Dictionary<string, object> arguments)
    {
        if (nameToken is null)
            throw new InvalidOperationException("Function name is null.");

        var attributes = MethodAttributes.Static;

        if (isPublic)
            attributes |= MethodAttributes.Public;

        var parameters = new List<TypeSignature>();
        var argts = new Dictionary<string, object>();

        foreach (var arg in args)
        {
            // TODO!
            parameters.Add(module.CorLibTypeFactory.Int32);
            argts.Add(arg.Text, 9);
        }

        var newMethod = new MethodDefinition(nameToken.Value.Text,
            attributes,
            MethodSignature.CreateStatic(module.CorLibTypeFactory.Void, parameters)); // TODO: Return value
        newMethod.CilMethodBody = new(newMethod);

        module.TopLevelTypes[1].Methods.Add(newMethod);

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
        foreach (var t in args) yield return new TokenNode(t);
        yield return block;
    }
}
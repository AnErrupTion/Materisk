using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.BuiltinTypes;
using Materisk.Native;

namespace Materisk.Parsing.Nodes;

internal class ModuleFunctionDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken moduleName;
    private readonly SyntaxToken name;
    private readonly Dictionary<SyntaxToken, SyntaxToken> args;
    private readonly SyntaxToken returnType;
    private readonly SyntaxNode body;
    private readonly bool isStatic;
    private readonly bool isPublic;
    private readonly bool isNative;

    public ModuleFunctionDefinitionNode(SyntaxToken moduleName, SyntaxToken name, Dictionary<SyntaxToken, SyntaxToken> args, SyntaxToken returnType, SyntaxNode body, bool isStatic, bool isPublic, bool isNative)
    {
        this.moduleName = moduleName;
        this.name = name;
        this.args = args;
        this.returnType = returnType;
        this.body = body;
        this.isStatic = isStatic;
        this.isPublic = isPublic;
        this.isNative = isNative;
    }

    public override NodeType Type => NodeType.ModuleFunctionDefinition;

    public override SValue Evaluate(Scope scope)
    {
        var targetName = name.Text;

        if (targetName is "ctor" or "toString") {
            if(args.Count(v => v.Value.Text == "self") != 1) {
                throw new Exception($"Special module method '{targetName}' must contain the argument 'self' exactly once.");
            }

            targetName = "$$" + targetName;
        }

        var fullName = $"{moduleName.Text}:{targetName}";

        SBaseFunction f;

        if (isNative)
        {
            f = new SNativeFunction(targetName, NativeFuncImpl.GetImplFor(fullName), args.Select(v => v.Value.Text).ToList(), !isStatic)
            {
                IsPublic = isPublic
            };
        }
        else
        {
            f = new SFunction(scope, targetName, args.Select(v => v.Value.Text).ToList(), body) 
            { 
                IsClassInstanceMethod = !isStatic,
                IsPublic = isPublic
            };
        }

        if (isPublic) scope.GetRoot().ExportTable.Add(fullName, f);
        return f;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var targetName = name.Text;

        if (targetName is "ctor" or "toString")
        {
            if(args.Count(v => v.Value.Text == "self") != 1)
                throw new Exception($"Special module method '{targetName}' must contain the argument 'self' exactly once.");

            targetName = "$$" + targetName;
        }

        MethodAttributes attributes = 0;

        if (isPublic)
            attributes |= MethodAttributes.Public;

        if (isStatic)
            attributes |= MethodAttributes.Static;

        var parameters = new List<TypeSignature>();
        var argts = new List<string>();

        foreach (var arg in args)
        {
            parameters.Add(Utils.GetTypeSignatureFor(module, arg.Key.Value.ToString()));
            argts.Add(arg.Value.Text);
        }

        var newMethod = new MethodDefinition(targetName,
            attributes,
            MethodSignature.CreateStatic(Utils.GetTypeSignatureFor(module, returnType.Value.ToString()), parameters));
        newMethod.CilMethodBody = new(newMethod);

        if (isNative)
            CilNativeFuncImpl.Emit(module, moduleName.Text, newMethod);
        else
            body.Emit(variables, module, newMethod, argts);

        newMethod.CilMethodBody.Instructions.Add(CilOpCodes.Ret);

        Console.WriteLine(newMethod.Name);
        foreach (var inst in newMethod.CilMethodBody.Instructions)
            Console.WriteLine(inst.ToString());
        Console.WriteLine("----");

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(name);
        foreach (var tok in args) yield return new TokenNode(tok.Value);
        yield return body;
    }
}
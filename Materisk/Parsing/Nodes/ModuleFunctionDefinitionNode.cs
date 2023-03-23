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
    private readonly SyntaxToken className;
    private readonly SyntaxToken name;
    private readonly List<SyntaxToken> args;
    private readonly SyntaxNode body;
    private readonly bool isStatic;
    private readonly bool isPublic;
    private readonly bool isNative;

    public ModuleFunctionDefinitionNode(SyntaxToken className, SyntaxToken name, List<SyntaxToken> args, SyntaxNode body, bool isStatic, bool isPublic, bool isNative)
    {
        this.className = className;
        this.name = name;
        this.args = args;
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
            if(args.Count(v => v.Text == "self") != 1) {
                throw new Exception($"Special module method '{targetName}' must contain the argument 'self' exactly once.");
            }

            targetName = "$$" + targetName;
        }

        var fullName = $"{className.Text}:{targetName}";

        SBaseFunction f;

        if (isNative)
        {
            f = new SNativeFunction(targetName, NativeFuncImpl.GetImplFor(fullName), args.Select(v => v.Text).ToList(), !isStatic)
            {
                IsPublic = isPublic
            };
        }
        else
        {
            f = new SFunction(scope, targetName, args.Select(v => v.Text).ToList(), body) 
            { 
                IsClassInstanceMethod = !isStatic,
                IsPublic = isPublic
            };
        }

        if (isPublic) scope.GetRoot().ExportTable.Add(fullName, f);
        return f;
    }

    // TODO: Native functions
    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, Dictionary<string, object> arguments)
    {
        var targetName = name.Text;

        if (targetName is "ctor" or "toString")
        {
            if(args.Count(v => v.Text == "self") != 1)
                throw new Exception($"Special module method '{targetName}' must contain the argument 'self' exactly once.");

            targetName = "$$" + targetName;
        }

        MethodAttributes attributes = 0;

        if (isPublic)
            attributes |= MethodAttributes.Public;

        if (isStatic)
            attributes |= MethodAttributes.Static;

        var parameters = new List<TypeSignature>();
        var argts = new Dictionary<string, object>();

        foreach (var arg in args)
        {
            // TODO!
            parameters.Add(module.CorLibTypeFactory.Int32);
            argts.Add(arg.Text, 9);
        }

        var newMethod = new MethodDefinition(targetName,
            attributes,
            MethodSignature.CreateStatic(module.CorLibTypeFactory.Void, parameters)); // TODO: Return value
        newMethod.CilMethodBody = new(newMethod);

        body.Emit(variables, module, newMethod, argts);

        newMethod.CilMethodBody.Instructions.Add(CilOpCodes.Ret);
        
        foreach (var inst in newMethod.CilMethodBody.Instructions)
            Console.WriteLine(inst.ToString());
        Console.WriteLine("----");

        return newMethod;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(name);
        foreach (var tok in args) yield return new TokenNode(tok);
        yield return body;
    }
}
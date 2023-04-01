using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using Materisk.Lex;

namespace Materisk.Parse.Nodes.Misc;

internal class InstantiateNode : SyntaxNode
{
    private readonly SyntaxToken _ident;
    private readonly List<SyntaxNode> _argumentNodes;

    public InstantiateNode(SyntaxToken ident, List<SyntaxNode> argumentNodes)
    {
        _ident = ident;
        _argumentNodes = argumentNodes;
    }

    public override NodeType Type => NodeType.Instantiate;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var name = _ident.Text;

        var constructorType = module.TopLevelTypes.FirstOrDefault(x => x.Name == name);
        if (constructorType == null)
            throw new InvalidOperationException($"Unable to find type with name: {name}");

        var constructor = constructorType.Methods.FirstOrDefault(x => x.Name == ".ctor");
        if (constructor is null)
            throw new InvalidOperationException($"Unable to find constructor of type: {name}");

        foreach (var arg in _argumentNodes)
            arg.Emit(variables, module, type, method, arguments);

        method.CilMethodBody!.Instructions.Add(CilOpCodes.Newobj, constructor);
        return null!;
    }

    public override object Emit(List<string> variables, LLVMModuleRef module, LLVMValueRef method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_ident);
        foreach (var node in _argumentNodes) yield return node;
    }
}
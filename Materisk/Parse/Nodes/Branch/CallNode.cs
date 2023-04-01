using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using LLVMSharp.Interop;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Branch;

internal class CallNode : SyntaxNode
{
    public SyntaxNode ToCallNode { get; }
    private readonly List<SyntaxNode> _argumentNodes;

    public CallNode(SyntaxNode atomNode, List<SyntaxNode> argumentNodes)
    {
        ToCallNode = atomNode;
        _argumentNodes = argumentNodes;
    }

    public override NodeType Type => NodeType.Call;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var toCall = ToCallNode.Emit(variables, module, type, method, arguments) as MethodDefinition;

        EmitArgs(variables, module, type, method, arguments);
        method.CilMethodBody!.Instructions.Add(CilOpCodes.Call, toCall);
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        var toCall = ToCallNode.Emit(module, type, method);
        var args = EmitArgs(module, type, method);
        return module.LlvmBuilder.BuildCall((LLVMValueRef)toCall, args.ToArray());
    }

    public void EmitArgs(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        foreach (var n in _argumentNodes)
            n.Emit(variables, module, type, method, arguments);
    }

    public List<LLVMValueRef> EmitArgs(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        var args = new List<LLVMValueRef>();

        foreach (var n in _argumentNodes)
            args.Add((LLVMValueRef)n.Emit(module, type, method));

        return args;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return ToCallNode;
        foreach (var n in _argumentNodes) yield return n;
    }

    public override string ToString()
    {
        return "CallNode:";
    }
}
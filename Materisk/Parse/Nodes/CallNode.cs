using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parse.Nodes;

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

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var toCall = ToCallNode.Emit(variables, module, type, method, arguments) as MethodDefinition;

        EmitArgs(variables, module, type, method, arguments);
        method.CilMethodBody.Instructions.Add(CilOpCodes.Call, toCall);
        return null;
    }

    public List<SValue> EvaluateArgs(Scope scope)
    {
        var args = new List<SValue>();

        foreach (var n in _argumentNodes) args.Add(n.Evaluate(scope));
        return args;
    }

    public void EmitArgs(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        foreach (var n in _argumentNodes)
            n.Emit(variables, module, type, method, arguments);
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
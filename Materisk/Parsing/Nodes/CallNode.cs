using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class CallNode : SyntaxNode
{
    public SyntaxNode ToCallNode { get; }
    private readonly List<SyntaxNode> argumentNodes;

    public CallNode(SyntaxNode atomNode, List<SyntaxNode> argumentNodes)
    {
        ToCallNode = atomNode;
        this.argumentNodes = argumentNodes;
    }

    public override NodeType Type => NodeType.Call;

    public override SValue Evaluate(Scope scope)
    {
        var toCall = ToCallNode.Evaluate(scope) ?? SValue.Null;
        var args = EvaluateArgs(scope);

        return toCall.Call(scope, args);
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public List<SValue> EvaluateArgs(Scope scope)
    {
        var args = new List<SValue>();

        foreach (var n in argumentNodes) args.Add(n.Evaluate(scope));
        return args;
    }

    public void EmitArgs(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var args = new List<object>();

        foreach (var n in argumentNodes)
            args.Add(n.Emit(variables, module, method, arguments));
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return ToCallNode;
        foreach (var n in argumentNodes) yield return n;
    }

    public override string ToString()
    {
        return "CallNode:";
    }
}
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class ReturnNode : SyntaxNode
{
    public ReturnNode()
    {
    }

    public ReturnNode(SyntaxNode returnValueNode)
    {
        ReturnValueNode = returnValueNode;
    }

    public SyntaxNode ReturnValueNode { get; }

    public override NodeType Type => NodeType.Return;

    public override SValue Evaluate(Scope scope)
    {
        scope.SetState(ScopeState.ShouldReturn);

        if (ReturnValueNode != null)
        {
            var v = ReturnValueNode.Evaluate(scope);
            scope.SetReturnValue(v);
        }

        return SValue.Null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, Dictionary<string, object> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (ReturnValueNode == null) yield break;
        yield return ReturnValueNode;
    }

    public override string ToString()
    {
        return "ReturnNode:";
    }
}
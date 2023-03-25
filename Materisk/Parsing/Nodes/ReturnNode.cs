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
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        ReturnValueNode?.Emit(variables, module, type, method, arguments);
        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (ReturnValueNode != null)
            yield return ReturnValueNode;
    }

    public override string ToString()
    {
        return "ReturnNode:";
    }
}
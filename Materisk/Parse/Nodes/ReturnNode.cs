using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;

namespace Materisk.Parse.Nodes;

internal class ReturnNode : SyntaxNode
{
    public ReturnNode(SyntaxNode? returnValueNode = null)
    {
        ReturnValueNode = returnValueNode;
    }

    public SyntaxNode? ReturnValueNode { get; }

    public override NodeType Type => NodeType.Return;

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
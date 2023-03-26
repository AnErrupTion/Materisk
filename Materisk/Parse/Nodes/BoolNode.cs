using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Materisk.Parse.Nodes;

public class BoolNode : SyntaxNode
{
    public override NodeType Type => NodeType.BooleanLiteral;

    public bool Value { get; }

    public BoolNode(bool value)
    {
        Value = value;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var value = Value ? 1 : 0;
        method.CilMethodBody!.Instructions.Add(CilInstruction.CreateLdcI4(value));
        return value;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }

    public override string ToString()
    {
        return "BoolNode: " + Value;
    }
}
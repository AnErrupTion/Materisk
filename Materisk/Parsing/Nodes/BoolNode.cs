using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

public class BoolNode : SyntaxNode
{
    public override NodeType Type => NodeType.BooleanLiteral;

    public bool Value { get; }

    public BoolNode(bool value)
    {
        Value = value;
    }

    public override SValue Evaluate(Scope scope)
    {
        return new SInt(Value ? 1 : 0);
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        throw new NotImplementedException();
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
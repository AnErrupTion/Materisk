using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;

namespace Materisk.Parse.Nodes;

public abstract class SyntaxNode
{
    public abstract NodeType Type { get; }

    public abstract object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments);

    public abstract IEnumerable<SyntaxNode> GetChildren();
}
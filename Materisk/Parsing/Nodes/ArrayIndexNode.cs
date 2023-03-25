using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

internal class ArrayIndexNode : SyntaxNode
{
    private readonly SyntaxNode nameNode;
    private readonly SyntaxNode indexNode;
    private readonly SyntaxNode? setNode;

    public ArrayIndexNode(SyntaxNode nameNode, SyntaxNode indexNode, SyntaxNode? setNode)
    {
        this.nameNode = nameNode;
        this.indexNode = indexNode;
        this.setNode = setNode;
    }

    public override NodeType Type => NodeType.ArrayIndex;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        if (nameNode.Emit(variables, module, type, method, arguments) is not CilLocalVariable variable)
            throw new InvalidOperationException("Catastrophic failure: variable is null."); // This should never happen

        indexNode.Emit(variables, module, type, method, arguments);

        // This should get the type of each element in the array.
        // For example: for "int[]" we'd get "int"
        var underlyingType = variable.VariableType.GetUnderlyingTypeDefOrRef();

        if (underlyingType is null)
            throw new InvalidOperationException("Catastrophic failure: variable is not array."); // This should never happen either

        if (setNode != null)
        {
            setNode.Emit(variables, module, type, method, arguments);
            method.CilMethodBody?.Instructions.Add(CilOpCodes.Stelem, underlyingType);
        } else method.CilMethodBody?.Instructions.Add(CilOpCodes.Ldelem, underlyingType);

        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return nameNode;
        yield return indexNode;
    }

    public override string ToString()
    {
        return "ArrayIndexNode:";
    }
}
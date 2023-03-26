using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parse.Nodes;

internal class ArrayIndexNode : SyntaxNode
{
    private readonly SyntaxNode _nameNode;
    private readonly SyntaxNode _indexNode;
    private readonly SyntaxNode? _setNode;

    public ArrayIndexNode(SyntaxNode nameNode, SyntaxNode indexNode, SyntaxNode? setNode)
    {
        _nameNode = nameNode;
        _indexNode = indexNode;
        _setNode = setNode;
    }

    public override NodeType Type => NodeType.ArrayIndex;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        if (_nameNode.Emit(variables, module, type, method, arguments) is not CilLocalVariable variable)
            throw new InvalidOperationException("Catastrophic failure: variable is null."); // This should never happen

        _indexNode.Emit(variables, module, type, method, arguments);

        // This should get the type of each element in the array.
        // For example: for "int[]" we'd get "int"
        var underlyingType = variable.VariableType.GetUnderlyingTypeDefOrRef();

        if (underlyingType is null)
            throw new InvalidOperationException("Catastrophic failure: variable is not array."); // This should never happen either

        if (_setNode != null)
        {
            _setNode.Emit(variables, module, type, method, arguments);
            method.CilMethodBody!.Instructions.Add(CilOpCodes.Stelem, underlyingType);
        } else method.CilMethodBody!.Instructions.Add(CilOpCodes.Ldelem, underlyingType);

        return null;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return _nameNode;
        yield return _indexNode;
    }

    public override string ToString()
    {
        return "ArrayIndexNode:";
    }
}
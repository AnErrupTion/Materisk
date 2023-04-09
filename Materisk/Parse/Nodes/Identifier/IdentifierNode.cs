using LLVMSharp.Interop;
using Materisk.TypeSystem;

namespace Materisk.Parse.Nodes.Identifier;

internal class IdentifierNode : SyntaxNode
{
    public readonly string Name;

    public IdentifierNode(string name)
    {
        Name = name;
    }

    public override NodeType Type => NodeType.Identifier;

    public override MateriskUnit Emit(MateriskModule module, MateriskType type, MateriskMethod method, LLVMBasicBlockRef thenBlock, LLVMBasicBlockRef elseBlock)
    {
        if (Name is "self" && !method.Attributes.HasFlag(MateriskAttributes.Static))
            return method.Arguments[0];

        foreach (var import in module.Imports)
            if (import.Key == Name)
                return import.Value;

        foreach (var typeDef in module.Types)
            if (typeDef.Name == Name)
                return typeDef;

        foreach (var field in module.Types[0].Fields)
            if (field.Name == Name)
                return field;

        foreach (var meth in module.Types[0].Methods)
            if (meth.Name == Name)
                return meth;

        foreach (var arg in method.Arguments)
            if (arg.Name == Name)
                return arg;

        foreach (var variable in method.Variables)
            if (variable.Name == Name)
                return variable;

        throw new InvalidOperationException($"Unable to find value for identifier: {Name}");
    }
}
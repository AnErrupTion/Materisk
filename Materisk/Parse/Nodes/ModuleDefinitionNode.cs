using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.BuiltinTypes;
using Materisk.Lex;

namespace Materisk.Parse.Nodes;

internal class ModuleDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken className;
    private readonly IEnumerable<SyntaxNode> body;
    private readonly bool isPublic;

    public ModuleDefinitionNode(SyntaxToken className, IEnumerable<SyntaxNode> body, bool isPublic)
    {
        this.className = className;
        this.body = body;
        this.isPublic = isPublic;
    }

    public override NodeType Type => NodeType.ModuleDefinition;

    public override SValue Evaluate(Scope scope)
    {
        return null;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        var attributes = TypeAttributes.Class;

        if (isPublic)
            attributes |= TypeAttributes.Public;

        var typeDef = new TypeDefinition(module.Name, className.Text, attributes, module.CorLibTypeFactory.Object.Type);

        module.TopLevelTypes.Add(typeDef);

        foreach (var bodyNode in body)
        {
            if (bodyNode is not ModuleFunctionDefinitionNode and not ModuleFieldDefinitionNode)
                throw new Exception($"Unexpected node in module definition: {bodyNode.GetType()}");

            bodyNode.Emit(variables, module, typeDef, method, arguments);
        }

        return typeDef;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(className);
        foreach (var n in body) yield return n;
    }
}
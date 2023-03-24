using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

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
        var @class = new SClass
        {
            Name = className.Text
        };

        foreach (var bodyNode in body)
        {
            if (bodyNode is not ModuleFunctionDefinitionNode cfdn) throw new Exception("Unexpected node in module definition");

            var funcRaw = cfdn.Evaluate(scope);

            if (funcRaw is not SBaseFunction func) throw new Exception("Expected ModuleFunctionDefinitionNode to return SBaseFunction");

            if (func.IsClassInstanceMethod)
            {
                @class.InstanceBaseTable.Add((new SString(func.FunctionName), func));
            }
            else
            {
                @class.StaticTable.Add((new SString(func.FunctionName), func));
            }
        }

        scope.Set(className.Text, @class);
        if (isPublic) scope.GetRoot().ExportTable.Add(className.Text, @class);
        return @class;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        var attributes = TypeAttributes.Class;

        if (isPublic)
            attributes |= TypeAttributes.Public;

        var type = new TypeDefinition(module.Name, className.Text, attributes);

        module.TopLevelTypes.Add(type);

        foreach (var bodyNode in body)
        {
            if (bodyNode is not ModuleFunctionDefinitionNode and not ModuleFieldDefinitionNode)
                throw new Exception($"Unexpected node in module definition: {bodyNode.GetType()}");

            var value = bodyNode.Emit(variables, module, method, arguments);

            switch (value)
            {
                case MethodDefinition methodDef: type.Methods.Add(methodDef); break;
                case FieldDefinition fieldDef: type.Fields.Add(fieldDef); break;
            }
        }

        return type;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(className);
        foreach (var n in body) yield return n;
    }
}
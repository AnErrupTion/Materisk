using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using MateriskLLVM;

namespace Materisk.Parse.Nodes.Definition;

internal class ModuleDefinitionNode : SyntaxNode
{
    private readonly string _name;
    private readonly IEnumerable<SyntaxNode> _body;
    private readonly bool _isPublic;

    public ModuleDefinitionNode(string name, IEnumerable<SyntaxNode> body, bool isPublic)
    {
        _name = name;
        _body = body;
        _isPublic = isPublic;
    }

    public override NodeType Type => NodeType.ModuleDefinition;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method, MateriskMetadata metadata)
    {
        var newType = new MateriskType(module, _name);

        module.Types.Add(newType);

        foreach (var bodyNode in _body)
        {
            if (bodyNode is not ModuleFunctionDefinitionNode and not ModuleFieldDefinitionNode)
                throw new Exception($"Unexpected node in module definition: {bodyNode.GetType()}");

            bodyNode.Emit(module, newType, method, metadata);
        }

        return newType;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return _body;
    }
}
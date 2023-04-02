using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using MateriskLLVM;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;

namespace Materisk.Parse.Nodes.Definition;

internal class ModuleDefinitionNode : SyntaxNode
{
    private readonly SyntaxToken _className;
    private readonly IEnumerable<SyntaxNode> _body;
    private readonly bool _isPublic;

    public ModuleDefinitionNode(SyntaxToken className, IEnumerable<SyntaxNode> body, bool isPublic)
    {
        _className = className;
        _body = body;
        _isPublic = isPublic;
    }

    public override NodeType Type => NodeType.ModuleDefinition;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        return null!;
    }

    public override object Emit(MateriskModule module, MateriskType type, MateriskMethod method)
    {
        var newType = new MateriskType(module, _className.Text);

        module.Types.Add(newType);

        foreach (var bodyNode in _body)
        {
            if (bodyNode is not ModuleFunctionDefinitionNode and not ModuleFieldDefinitionNode)
                throw new Exception($"Unexpected node in module definition: {bodyNode.GetType()}");

            bodyNode.Emit(module, newType, method);
        }

        return newType;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_className);
        foreach (var n in _body) yield return n;
    }
}
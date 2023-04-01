using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using LLVMSharp.Interop;
using Materisk.Lex;
using Materisk.Parse.Nodes.Misc;
using Materisk.Utils;

namespace Materisk.Parse.Nodes.Definition;

internal class ModuleFieldDefinitionNode : SyntaxNode
{
    private readonly bool _isPublic;
    private readonly bool _isStatic;
    private readonly SyntaxToken _nameToken;
    private readonly SyntaxToken _typeToken;

    public ModuleFieldDefinitionNode(bool isPublic, bool isStatic, SyntaxToken nameToken, SyntaxToken typeToken)
    {
        _isPublic = isPublic;
        _isStatic = isStatic;
        _nameToken = nameToken;
        _typeToken = typeToken;
    }

    public override NodeType Type => NodeType.ModuleFieldDefinition;

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, TypeDefinition type, MethodDefinition method, List<string> arguments)
    {
        FieldAttributes attributes = 0;

        if (_isPublic)
            attributes |= FieldAttributes.Public;

        if (_isStatic)
            attributes |= FieldAttributes.Static;

        var newField = new FieldDefinition(_nameToken.Text,
            attributes,
            TypeSigUtils.GetTypeSignatureFor(module, _typeToken.Text));

        type.Fields.Add(newField);

        return newField;
    }

    public override object Emit(List<string> variables, LLVMModuleRef module, LLVMValueRef method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return new TokenNode(_nameToken);
        yield return new TokenNode(_typeToken);
    }
}
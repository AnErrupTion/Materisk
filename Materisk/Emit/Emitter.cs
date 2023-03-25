using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.Parsing.Nodes;

namespace Materisk.Emit;

public class Emitter
{
    private readonly string _moduleName;
    private readonly string _outputPath;
    private readonly SyntaxNode _rootNode;

    public Emitter(string moduleName, string outputPath, SyntaxNode rootNode)
    {
        _moduleName = moduleName;
        _outputPath = outputPath;
        _rootNode = rootNode;
    }

    public void Emit(EmitType emitType)
    {
        var assembly = new AssemblyDefinition(_moduleName, new Version(1, 0, 0, 0));

        var module = new ModuleDefinition(_moduleName);
        assembly.Modules.Add(module);

        var mainType = new TypeDefinition(_moduleName, "Program", TypeAttributes.Public | TypeAttributes.Class, module.CorLibTypeFactory.Object.Type);
        module.TopLevelTypes.Add(mainType);

        var variables = new Dictionary<string, CilLocalVariable>();
        _rootNode.Emit(variables, module, null, null, null);

        assembly.Write(_outputPath);
    }
}
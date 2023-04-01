using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Materisk.Parse.Nodes;
using Newtonsoft.Json;
using FileAttributes = AsmResolver.PE.DotNet.Metadata.Tables.Rows.FileAttributes;

namespace Materisk.Emit;

public class Emitter
{
    private readonly string _moduleName;
    private readonly SyntaxNode _rootNode;

    public Emitter(string moduleName, SyntaxNode rootNode)
    {
        _moduleName = moduleName;
        _rootNode = rootNode;
    }

    public void Emit(EmitType emitType, string customCoreLib, List<string> references)
    {
        if (emitType is EmitType.Cil)
        {
            // Generate assembly
            var assembly = new AssemblyDefinition(_moduleName, new Version(1, 0, 0, 0));
            var module = new ModuleDefinition(_moduleName,
                string.IsNullOrEmpty(customCoreLib)
                ? KnownCorLibs.MsCorLib_v4_0_0_0
                : new AssemblyReference(AssemblyDefinition.FromFile(customCoreLib)));

            foreach (var reference in references)
                module.FileReferences.Add(new(reference, FileAttributes.ContainsMetadata));

            assembly.Modules.Add(module);

            var mainType = new TypeDefinition(_moduleName, "Program", TypeAttributes.Public | TypeAttributes.Class, module.CorLibTypeFactory.Object.Type);
            module.TopLevelTypes.Add(mainType);

            var variables = new Dictionary<string, CilLocalVariable>();
            _rootNode.Emit(variables, module, null!, null!, null!);

            assembly.Write($"{_moduleName}.dll");

            // Generate runtimeconfig.json file
            using var stream = File.Create($"{_moduleName}.runtimeconfig.json");
            using var writer = new StreamWriter(stream);
            using var runtimeConfig = new JsonTextWriter(writer);

            runtimeConfig.WriteStartObject();
            {
                runtimeConfig.WritePropertyName("runtimeOptions");
                runtimeConfig.WriteStartObject();
                {
                    runtimeConfig.WritePropertyName("tfm");
                    runtimeConfig.WriteValue("net7.0");
                    runtimeConfig.WritePropertyName("framework");
                    runtimeConfig.WriteStartObject();
                    {
                        runtimeConfig.WritePropertyName("name");
                        runtimeConfig.WriteValue("Microsoft.NETCore.App");
                        runtimeConfig.WritePropertyName("version");
                        runtimeConfig.WriteValue("7.0.0");
                    }
                    runtimeConfig.WriteEndObject();
                }
                runtimeConfig.WriteEndObject();
            }
            runtimeConfig.WriteEndObject();

            runtimeConfig.Flush();
        }
    }
}
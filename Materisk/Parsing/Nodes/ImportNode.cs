using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using Materisk.BuiltinTypes;

namespace Materisk.Parsing.Nodes;

public class ImportNode : SyntaxNode
{
    private readonly SyntaxToken path;

    public ImportNode(SyntaxToken path) {
        this.path = path;
    }

    public override NodeType Type => NodeType.Import;

    public override SValue Evaluate(Scope scope) {
        if (!File.Exists(path.Text)) throw new Exception($"Failed to import '{path.Text}': File not found");
        var text = File.ReadAllText(path.Text);

        Interpreter ip = new();
        InterpreterResult res = new();

        try {
            ip.Interpret(text, ref res);

            // copy export table

            foreach(var kvp in ip.GlobalScope.ExportTable) {
                if (scope.Get(kvp.Key) != null) throw new Exception($"Failed to import '{path.Text}': Import conflict; file exports '{kvp.Key}' but that identifier is already present in the current scope.");

                scope.Set(kvp.Key, kvp.Value);
            }
        }catch(Exception ex) {
            throw new Exception($"Failed to import '{path.Text}': {ex.Message}");
        }

        return res.LastValue;
    }

    public override object Emit(Dictionary<string, CilLocalVariable> variables, ModuleDefinition module, MethodDefinition method, List<string> arguments)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<SyntaxNode> GetChildren() {
        throw new NotImplementedException();
    }
}
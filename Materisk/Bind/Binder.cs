using Materisk.Parse.Nodes;
using Materisk.Utils;

namespace Materisk.Bind;

public sealed class Binder
{
    private readonly string _path;
    private readonly SyntaxNode _rootNode;
    private readonly List<Diagnostic> _diagnostics;

    public Binder(string path, SyntaxNode rootNode, List<Diagnostic> diagnostics)
    {
        _path = path;
        _rootNode = rootNode;
        _diagnostics = diagnostics;
    }

    public SyntaxNode Bind()
    {
        return _rootNode;
    }
}
using Materisk.Lex;

namespace Materisk.Utils;

public sealed class Diagnostic
{
    public readonly string Text;

    private Diagnostic(string text) => Text = text;

    public static Diagnostic Create(string path, SyntaxToken token, string text)
        => new($"{path}: position {token.Position}, text \"{token.Text}\": {text}");
}
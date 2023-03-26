namespace Materisk.Lex;

public sealed class SyntaxToken
{
    public readonly SyntaxType Type;
    public readonly int Position;
    public readonly string Text;

    public SyntaxToken(SyntaxType type, int pos, string txt)
    {
        Type = type;
        Position = pos;
        Text = txt;
    }

    public override string ToString() => $"{Type} at position {Position} with text: {Text}";
}
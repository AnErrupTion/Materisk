namespace Materisk.Lex;

public sealed class SyntaxToken
{
    public SyntaxType Type { get; set; }

    public int Position { get; }

    public string Text { get; }

    public SyntaxToken(SyntaxType type, int pos, string txt)
    {
        Type = type;
        Position = pos;
        Text = txt;
    }

    public override string ToString()
        => Type.ToString().PadRight(16) + " at " + Position.ToString().PadRight(3) + " with text: " + Text.PadRight(16);
}
namespace Materisk.Lex;

public static class SyntaxFacts
{
    public static void ClassifyIdentifier(ref SyntaxToken token)
    {
        if (token.Text is "return" or "continue" or "break"
            or "if" or "elif" or "else"
            or "for" or "while" or "fn" or "var" or "fld"
            or "import" or "native" or "new"
            or "mod" or "dyn" or "pub")
            token.Type = SyntaxType.Keyword;
    }
}
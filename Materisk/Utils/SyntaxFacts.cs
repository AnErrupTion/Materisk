namespace Materisk.Utils;

public static class SyntaxFacts
{
    public static bool IsKeyword(string text) => text
        is "return" or "continue" or "break"
        or "if" or "elif" or "else"
        or "for" or "while" or "fn" or "var" or "fld" or "mut"
        or "using" or "native" or "new"
        or "mod" or "dyn" or "pub"
        or "true" or "false";
}
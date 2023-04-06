using Materisk.Lex;

namespace Materisk.Utils;

public static class PrecedenceUtils
{
    public static int GetBinaryOperatorPrecedence(this SyntaxType type)
    {
        return type switch
        {
            SyntaxType.Mul or SyntaxType.Div => 5,
            SyntaxType.Plus or SyntaxType.Minus => 4,
            SyntaxType.EqualsEquals or SyntaxType.BangEquals or SyntaxType.LessThan or SyntaxType.LessThanEqu or SyntaxType.GreaterThan or SyntaxType.GreaterThanEqu => 3,
            SyntaxType.AndAnd => 2,
            SyntaxType.OrOr => 1,
            _ => 0
        };
    }

    public static int GetUnaryOperatorPrecedence(this SyntaxType type)
    {
        return type switch
        {
            SyntaxType.Plus or SyntaxType.Minus or SyntaxType.Bang => 6,
            _ => 0
        };
    }
}
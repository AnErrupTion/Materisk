namespace Materisk.Lex;

public enum SyntaxType
{
    BadToken,
    Semicolon,
    Keyword,
    Identifier,
    Equals,
    PlusEquals, MinusEquals, ModEquals, MulEquals, DivEquals, PlusPlus, MinusMinus,
    EqualsEquals, AndAnd, OrOr,
    LessThan, GreaterThan, GreaterThanEqu, LessThanEqu,
    Plus, Minus, Mod, Mul, Div, Pow,
    Dot,
    LParen, RParen,
    Int, Float, String,
    LSqBracket, RSqBracket,
    LBraces, RBraces,
    Bang,
    Comma,
    Eof
}
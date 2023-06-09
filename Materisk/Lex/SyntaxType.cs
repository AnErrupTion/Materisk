namespace Materisk.Lex;

public enum SyntaxType
{
    BadToken,
    Semicolon,
    Keyword,
    Identifier,
    Equals,
    PlusEquals, MinusEquals, ModEquals, MulEquals, DivEquals,
    BangEquals, EqualsEquals, AndAnd, OrOr,
    LessThan, GreaterThan, GreaterThanEqu, LessThanEqu,
    Plus, Minus, Mod, Mul, Div,
    Dot, PointerDot,
    LParen, RParen,
    Decimal, Hexadecimal, Binary,
    String,
    LSqBracket, RSqBracket,
    LBraces, RBraces,
    Bang,
    Comma,
    Eof
}
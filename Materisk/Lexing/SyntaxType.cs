namespace Materisk.Lexing;

public enum SyntaxType
{
    Semicolon,
    Keyword,
    Identifier,
    Equals,
    EqualsEquals,
    AndAnd,
    OrOr,
    LessThan,
    GreaterThan,
    GreaterThanEqu,
    LessThanEqu,
    Plus, Minus,
    Mod, Mul, Div, Idx,
    Pow,
    Dot,
    LParen,
    RParen,
    Int,
    Float,
    String,
    LSqBracket,
    RSqBracket,
    LBraces,
    RBraces,
    Bang,
    EOF,
    BadToken,
    Comma
}
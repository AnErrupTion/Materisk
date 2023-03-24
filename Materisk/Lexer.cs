﻿using System.Globalization;

namespace Materisk;

public class Lexer {
    public string Code { get; }
    public int Position { get; set; }

    public char Current => Peek();

    public char Peek(int off = 0) {
        if (Position + off >= Code.Length || Position + off < 0) return '\0';
        return Code[Position + off];
    }

    public Lexer(string code) {
        Code = code;
    }

    public List<SyntaxToken> Lex() {
        List<SyntaxToken> tokens = new();

        while(Current != '\0') {
            SyntaxToken insertToken = new(SyntaxType.BadToken, Position, null, Current.ToString());
            switch(Current) {
                case ';':
                    insertToken = (new(SyntaxType.Semicolon, Position, null, Current.ToString()));
                    break;
                case '=':
                    if (Peek(1) == '=') {
                        Position++;
                        insertToken = (new(SyntaxType.EqualsEquals, Position, null, "=="));
                    }else {
                        insertToken = (new(SyntaxType.Equals, Position, null, Current.ToString()));
                    }

                    break;
                case '<':
                    if (Peek(1) == '=') {
                        Position++;
                        insertToken = (new(SyntaxType.LessThanEqu, Position, null, "<="));
                    } else {
                        insertToken = (new(SyntaxType.LessThan, Position, null, Current.ToString()));
                    }

                    break;
                case '>':
                    if (Peek(1) == '=') {
                        Position++;
                        insertToken = (new(SyntaxType.GreaterThanEqu, Position, null, ">="));
                    } else {
                        insertToken = (new(SyntaxType.GreaterThan, Position, null, Current.ToString()));
                    }

                    break;
                case '|':
                    if (Peek(1) == '|') {
                        Position++;
                        insertToken = (new(SyntaxType.OrOr, Position, null, "||"));
                    } else {
                        insertToken = (new(SyntaxType.BadToken, Position, null, Current.ToString()));
                    }

                    break;
                case '&':
                    if (Peek(1) == '&') {
                        Position++;
                        insertToken = (new(SyntaxType.AndAnd, Position, null, "&&"));
                    } else {
                        insertToken = (new(SyntaxType.BadToken, Position, null, Current.ToString()));
                    }

                    break;
                case '+':
                    insertToken = (new(SyntaxType.Plus, Position, null, Current.ToString()));
                    break;
                case '-':
                    insertToken = (new(SyntaxType.Minus, Position, null, Current.ToString()));
                    break;
                case '%':
                    insertToken = (new(SyntaxType.Mod, Position, null, Current.ToString()));
                    break;
                case '*':
                    insertToken = (new(SyntaxType.Mul, Position, null, Current.ToString()));
                    break;
                case '/':
                    if(Peek(1) == '/') {
                        SkipComment();
                        continue;
                    }
                    insertToken = (new(SyntaxType.Div, Position, null, Current.ToString()));
                    break;
                case '#':
                    insertToken = (new(SyntaxType.Idx, Position, null, Current.ToString()));
                    break;
                case '.':
                    insertToken = (new(SyntaxType.Dot, Position, null, Current.ToString()));
                    break;
                case ',':
                    insertToken = (new(SyntaxType.Comma, Position, null, Current.ToString()));
                    break;
                case '(':
                    insertToken = (new(SyntaxType.LParen, Position, null, Current.ToString()));
                    break;
                case ')':
                    insertToken = (new(SyntaxType.RParen, Position, null, Current.ToString()));
                    break;
                case '[':
                    insertToken = (new(SyntaxType.LSqBracket, Position, null, Current.ToString()));
                    break;
                case ']':
                    insertToken = (new(SyntaxType.RSqBracket, Position, null, Current.ToString()));
                    break;
                case '{':
                    insertToken = (new(SyntaxType.LBraces, Position, null, Current.ToString()));
                    break;
                case '}':
                    insertToken = (new(SyntaxType.RBraces, Position, null, Current.ToString()));
                    break;
                case '!':
                    insertToken = (new(SyntaxType.Bang, Position, null, Current.ToString()));
                    break;
            }

            if (insertToken.Type == SyntaxType.BadToken) {
                if (char.IsDigit(Current)) {
                    tokens.Add(ParseNumber());
                } else if (Current == '"') {
                    tokens.Add(ParseString());
                } else if (char.IsLetter(Current)) {
                    tokens.Add(ParseIdentifierOrKeyword());
                } else if (char.IsWhiteSpace(Current)) Position++;
                else {
                    throw new Exception("Bad token at pos " + insertToken.Position + " with text " + insertToken.Text);
                }
            } else {
                tokens.Add(insertToken);
                Position++;
            }
        }

        tokens.Add(new SyntaxToken(SyntaxType.EOF, Position, null, "<EOF>"));
        return tokens;
    }

    private void SkipComment() {
        while(Current != '\0' && Current != '\n') {
            Position++;
        }
    }

    private SyntaxToken ParseIdentifierOrKeyword() {
        var str = string.Empty;

        while (Current != '\0' && Current != ' ' && (char.IsLetterOrDigit(Current) || Current == '_')) {
            str += Current;
            Position++;
        }

        var token = new SyntaxToken(SyntaxType.Identifier, Position, str, str);
        SyntaxFacts.ClassifyIdentifier(ref token);

        return token;
    }


    private SyntaxToken ParseString() {
        var str = string.Empty;

        Position++;
        while(!(Current == '"' && Peek(-1) != '\\') && Current != '\0') {
            if (Current == '\\') {
                Position++;

                switch (Current) {
                    case '"': str += "\""; break;
                    case 'n': str += "\n"; break;
                    case '\\': str += "\\"; break;
                    default: throw new Exception("Invalid escape sequence");
                }

                Position++;
            } else {
                str += Current;
                Position++;
            }
        }

        Position++;
        return new(SyntaxType.String, Position-1, str, str);
    }

    private SyntaxToken ParseNumber() {
        var numStr = string.Empty;
        var isDecimal = false;

        while((char.IsDigit(Current) || Current == '.') && Current != '\0') {
            numStr += Current;

            if(Current == '.') {
                isDecimal = true;
            }

            Position++;
        }

        if(isDecimal) {
            if (!float.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatVal)) throw new Exception("Invalid number (tried to parse " + numStr + " as float)");
            return new(SyntaxType.Float, Position-1, floatVal, numStr);
        }
        if (!int.TryParse(numStr, out var intVal)) throw new Exception("Invalid number!");
        return new(SyntaxType.Int, Position-1, intVal, numStr);
    }
}

public struct SyntaxToken {
    public SyntaxType Type { get; set; }
    public int Position { get; }
    public object Value { get; }
    public string Text { get; }

    public SyntaxToken(SyntaxType type, int pos, object val, string txt) {
        Type = type;
        Position = pos;
        Value = val;
        Text = txt;
    }

    public override string ToString() {
        return Type.ToString().PadRight(16) + " at " + Position.ToString().PadRight(3) + " with val: " + (Value ?? "null").ToString().PadRight(16) + " text: " + Text.PadRight(16);
    }
}

public enum SyntaxType {
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
    Comma,
}

public static class SyntaxFacts {
    public static void ClassifyIdentifier(ref SyntaxToken token) {
        if (token.Text is "return" or "continue" or "break"
            or "if" or "elif" or "else"
            or "for" or "while" or "fn" or "var" or "fld"
            or "import" or "native" or "new"
            or "mod" or "dyn" or "pub") {
            token.Type = SyntaxType.Keyword;
        }
    }
}
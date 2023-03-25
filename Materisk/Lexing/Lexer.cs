﻿namespace Materisk.Lexing;

public class Lexer
{
    public string Code { get; }

    public int Position { get; set; }

    public char Current => Peek();

    public char Peek(int off = 0)
    {
        if (Position + off >= Code.Length || Position + off < 0)
            return '\0';
        return Code[Position + off];
    }

    public Lexer(string code)
    {
        Code = code;
    }

    public List<SyntaxToken> Lex()
    {
        var tokens = new List<SyntaxToken>();

        while (Current != '\0')
        {
            var insertToken = new SyntaxToken(SyntaxType.BadToken, Position, Current.ToString());
            switch (Current)
            {
                case ';':
                    insertToken = new(SyntaxType.Semicolon, Position, Current.ToString());
                    break;
                case '=':
                    if (Peek(1) == '=') {
                        Position++;
                        insertToken = new(SyntaxType.EqualsEquals, Position, "==");
                    }else {
                        insertToken = new(SyntaxType.Equals, Position, Current.ToString());
                    }

                    break;
                case '<':
                    if (Peek(1) == '=') {
                        Position++;
                        insertToken = new(SyntaxType.LessThanEqu, Position, "<=");
                    } else {
                        insertToken = new(SyntaxType.LessThan, Position, Current.ToString());
                    }

                    break;
                case '>':
                    if (Peek(1) == '=') {
                        Position++;
                        insertToken = new(SyntaxType.GreaterThanEqu, Position, ">=");
                    } else {
                        insertToken = new(SyntaxType.GreaterThan, Position, Current.ToString());
                    }

                    break;
                case '|':
                    if (Peek(1) == '|') {
                        Position++;
                        insertToken = new(SyntaxType.OrOr, Position, "||");
                    } else {
                        insertToken = new(SyntaxType.BadToken, Position, Current.ToString());
                    }

                    break;
                case '&':
                    if (Peek(1) == '&') {
                        Position++;
                        insertToken = new(SyntaxType.AndAnd, Position, "&&");
                    } else {
                        insertToken = new(SyntaxType.BadToken, Position, Current.ToString());
                    }

                    break;
                case '+':
                    insertToken = new(SyntaxType.Plus, Position, Current.ToString());
                    break;
                case '-':
                    insertToken = new(SyntaxType.Minus, Position, Current.ToString());
                    break;
                case '%':
                    insertToken = new(SyntaxType.Mod, Position, Current.ToString());
                    break;
                case '*':
                    insertToken = new(SyntaxType.Mul, Position, Current.ToString());
                    break;
                case '/':
                    if(Peek(1) == '/') {
                        SkipComment();
                        continue;
                    }
                    insertToken = new(SyntaxType.Div, Position, Current.ToString());
                    break;
                case '#':
                    insertToken = new(SyntaxType.Idx, Position, Current.ToString());
                    break;
                case '.':
                    insertToken = new(SyntaxType.Dot, Position, Current.ToString());
                    break;
                case ',':
                    insertToken = new(SyntaxType.Comma, Position, Current.ToString());
                    break;
                case '(':
                    insertToken = new(SyntaxType.LParen, Position, Current.ToString());
                    break;
                case ')':
                    insertToken = new(SyntaxType.RParen, Position, Current.ToString());
                    break;
                case '[':
                    insertToken = new(SyntaxType.LSqBracket, Position, Current.ToString());
                    break;
                case ']':
                    insertToken = new(SyntaxType.RSqBracket, Position, Current.ToString());
                    break;
                case '{':
                    insertToken = new(SyntaxType.LBraces, Position, Current.ToString());
                    break;
                case '}':
                    insertToken = new(SyntaxType.RBraces, Position, Current.ToString());
                    break;
                case '!':
                    insertToken = new(SyntaxType.Bang, Position, Current.ToString());
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

        tokens.Add(new SyntaxToken(SyntaxType.EOF, Position, "<EOF>"));
        return tokens;
    }

    private void SkipComment()
    {
        while(Current != '\0' && Current != '\n')
            Position++;
    }

    private SyntaxToken ParseIdentifierOrKeyword()
    {
        var str = string.Empty;

        while (Current != '\0' && Current != ' ' && (char.IsLetterOrDigit(Current) || Current == '_'))
        {
            str += Current;
            Position++;
        }

        var token = new SyntaxToken(SyntaxType.Identifier, Position, str);
        SyntaxFacts.ClassifyIdentifier(ref token);

        return token;
    }

    private SyntaxToken ParseString()
    {
        var str = string.Empty;

        Position++;
        while (!(Current == '"' && Peek(-1) != '\\') && Current != '\0')
        {
            if (Current == '\\')
            {
                Position++;

                str += Current switch
                {
                    '"' => "\"",
                    'n' => "\n",
                    '\\' => "\\",
                    _ => throw new Exception("Invalid escape sequence")
                };

                Position++;
            }
            else
            {
                str += Current;
                Position++;
            }
        }

        Position++;
        return new(SyntaxType.String, Position-1, str);
    }

    private SyntaxToken ParseNumber()
    {
        var numStr = string.Empty;
        var isDecimal = false;

        while ((char.IsDigit(Current) || Current == '.') && Current != '\0')
        {
            numStr += Current;

            if(Current == '.')
                isDecimal = true;

            Position++;
        }

        return new(isDecimal ? SyntaxType.Float : SyntaxType.Int, Position - 1, numStr);
    }
}
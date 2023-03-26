namespace Materisk.Lex;

public class Lexer
{
    private readonly string _code;
    private int _position;

    public Lexer(string code)
    {
        _code = code;
        _position = 0;
    }

    public SyntaxToken[] Lex()
    {
        var tokens = new List<SyntaxToken>();
        char current;

        while ((current = Peek()) != '\0')
        {
            SyntaxToken? insertToken = null;

            switch (current)
            {
                case ';':
                {
                    insertToken = new(SyntaxType.Semicolon, _position, ";");
                    break;
                }
                case '=':
                {
                    if (Peek(1) == '=')
                    {
                        _position++;
                        insertToken = new(SyntaxType.EqualsEquals, _position, "==");
                    }
                    else insertToken = new(SyntaxType.Equals, _position, "=");

                    break;
                }
                case '<':
                {
                    if (Peek(1) == '=')
                    {
                        _position++;
                        insertToken = new(SyntaxType.LessThanEqu, _position, "<=");
                    }
                    else insertToken = new(SyntaxType.LessThan, _position, "<");

                    break;
                }
                case '>':
                {
                    if (Peek(1) == '=')
                    {
                        _position++;
                        insertToken = new(SyntaxType.GreaterThanEqu, _position, ">=");
                    }
                    else insertToken = new(SyntaxType.GreaterThan, _position, ">");

                    break;
                }
                case '|':
                {
                    var lookAhead = Peek(1);

                    if (lookAhead == '|')
                    {
                        _position++;
                        insertToken = new(SyntaxType.OrOr, _position, "||");
                    } else insertToken = new(SyntaxType.BadToken, _position, $"|{lookAhead}");

                    break;
                }
                case '&':
                {
                    var lookAhead = Peek(1);

                    if (lookAhead == '&')
                    {
                        _position++;
                        insertToken = new(SyntaxType.AndAnd, _position, "&&");
                    } else insertToken = new(SyntaxType.BadToken, _position, $"&{lookAhead}");

                    break;
                }
                case '+':
                {
                    insertToken = new(SyntaxType.Plus, _position, "+");
                    break;
                }
                case '-':
                {
                    insertToken = new(SyntaxType.Minus, _position, "-");
                    break;
                }
                case '%':
                {
                    insertToken = new(SyntaxType.Mod, _position, "%");
                    break;
                }
                case '*':
                {
                    insertToken = new(SyntaxType.Mul, _position, "*");
                    break;
                }
                case '/':
                {
                    if (Peek(1) == '/')
                    {
                        SkipComment();
                        continue;
                    }

                    insertToken = new(SyntaxType.Div, _position, "/");
                    break;
                }
                case '.':
                {
                    insertToken = new(SyntaxType.Dot, _position, ".");
                    break;
                }
                case ',':
                {
                    insertToken = new(SyntaxType.Comma, _position, ",");
                    break;
                }
                case '(':
                {
                    insertToken = new(SyntaxType.LParen, _position, "(");
                    break;
                }
                case ')':
                {
                    insertToken = new(SyntaxType.RParen, _position, ")");
                    break;
                }
                case '[':
                {
                    insertToken = new(SyntaxType.LSqBracket, _position, "[");
                    break;
                }
                case ']':
                {
                    insertToken = new(SyntaxType.RSqBracket, _position, "]");
                    break;
                }
                case '{':
                {
                    insertToken = new(SyntaxType.LBraces, _position, "{");
                    break;
                }
                case '}':
                {
                    insertToken = new(SyntaxType.RBraces, _position, "}");
                    break;
                }
                case '!':
                {
                    insertToken = new(SyntaxType.Bang, _position, "!");
                    break;
                }
            }

            if (insertToken is null || insertToken.Type is SyntaxType.BadToken)
            {
                if (char.IsDigit(current))
                    tokens.Add(ParseNumber());
                else if (current == '"')
                    tokens.Add(ParseString());
                else if (char.IsLetter(current))
                    tokens.Add(ParseIdentifierOrKeyword());
                else if (char.IsWhiteSpace(current))
                    _position++;
                else
                    throw new Exception($"Bad token at position {_position} with text: {current}");

                continue;
            }

            tokens.Add(insertToken);
            _position++;
        }

        tokens.Add(new SyntaxToken(SyntaxType.Eof, _position, "<EOF>"));
        return tokens.ToArray();
    }

    private char Peek(int off = 0)
    {
        if (_position + off >= _code.Length || _position + off < 0)
            return '\0';
        return _code[_position + off];
    }

    private void SkipComment()
    {
        while (Peek() != '\0' && Peek() != '\n')
            _position++;
    }

    private SyntaxToken ParseIdentifierOrKeyword()
    {
        var str = string.Empty;

        while (Peek() != '\0' && Peek() != ' ' && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            str += Peek();
            _position++;
        }

        var token = new SyntaxToken(SyntaxType.Identifier, _position, str);
        SyntaxFacts.ClassifyIdentifier(ref token);

        return token;
    }

    private SyntaxToken ParseString()
    {
        char current;

        var str = string.Empty;

        _position++;
        while (!((current = Peek()) == '"' && Peek(-1) != '\\') && current != '\0')
        {
            if (current == '\\')
            {
                _position++;

                str += Peek() switch
                {
                    '"' => "\"",
                    'n' => "\n",
                    '\\' => "\\",
                    _ => throw new Exception("Invalid escape sequence")
                };

                _position++;
            }
            else
            {
                str += current;
                _position++;
            }
        }

        _position++;
        return new(SyntaxType.String, _position - 1, str);
    }

    private SyntaxToken ParseNumber()
    {
        char current;

        var numStr = string.Empty;
        var isDecimal = false;

        while ((char.IsDigit(current = Peek()) || current == '.') && current != '\0')
        {
            numStr += current;

            if (current == '.')
                isDecimal = true;

            _position++;
        }

        return new(isDecimal ? SyntaxType.Float : SyntaxType.Int, _position - 1, numStr);
    }
}
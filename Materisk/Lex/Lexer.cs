using System.Text;
using Materisk.Utils;

namespace Materisk.Lex;

public sealed class Lexer
{
    private readonly string _path;
    private readonly List<Diagnostic> _diagnostics;
    private readonly string _code;
    private readonly StringBuilder _builder;

    private int _position;

    public Lexer(string path, List<Diagnostic> diagnostics)
    {
        _path = path;
        _code = File.ReadAllText(path);
        _diagnostics = diagnostics;
        _builder = new();
        _position = 0;
    }

    public List<SyntaxToken> Lex()
    {
        char current;

        var tokens = new List<SyntaxToken>();

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
                    var lookAhead = Peek(1);

                    if (lookAhead == '=')
                    {
                        _position++;
                        insertToken = new(SyntaxType.PlusEquals, _position, "+=");
                    }
                    else insertToken = new(SyntaxType.Plus, _position, "+");

                    break;
                }
                case '-':
                {
                    var lookAhead = Peek(1);

                    if (lookAhead == '=')
                    {
                        _position++;
                        insertToken = new(SyntaxType.MinusEquals, _position, "-=");
                    }
                    else if (lookAhead == '>')
                    {
                        _position++;
                        insertToken = new(SyntaxType.PointerDot, _position, "->");
                    }
                    else insertToken = new(SyntaxType.Minus, _position, "-");

                    break;
                }
                case '%':
                {
                    var lookAhead = Peek(1);

                    if (lookAhead == '=')
                    {
                        _position++;
                        insertToken = new(SyntaxType.ModEquals, _position, "%=");
                    }
                    else insertToken = new(SyntaxType.Mod, _position, "%");

                    break;
                }
                case '*':
                {
                    var lookAhead = Peek(1);

                    if (lookAhead == '=')
                    {
                        _position++;
                        insertToken = new(SyntaxType.MulEquals, _position, "*=");
                    }
                    else insertToken = new(SyntaxType.Mul, _position, "*");

                    break;
                }
                case '/':
                {
                    var lookAhead = Peek(1);

                    if (lookAhead == '/')
                    {
                        SkipComment();
                        continue;
                    }

                    if (lookAhead == '=')
                    {
                        _position++;
                        insertToken = new(SyntaxType.DivEquals, _position, "/=");
                    }
                    else insertToken = new(SyntaxType.Div, _position, "/");

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
                    var lookAhead = Peek(1);

                    if (lookAhead == '=')
                    {
                        _position++;
                        insertToken = new(SyntaxType.BangEquals, _position, "!=");
                    } else insertToken = new(SyntaxType.Bang, _position, "!");

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
                    _diagnostics.Add(Diagnostic.Create(_path, CreateBadToken(current.ToString()), "Bad character"));

                continue;
            }

            tokens.Add(insertToken);
            _position++;
        }

        tokens.Add(new SyntaxToken(SyntaxType.Eof, _position, "<EOF>"));
        return tokens;
    }

    private char Peek(int off = 0)
    {
        var offset = _position + off;
        if (offset >= 0 && offset < _code.Length)
            return _code[offset];
        return '\0';
    }

    private void SkipComment()
    {
        while (Peek() != '\0' && Peek() != '\n')
            _position++;
    }

    private SyntaxToken ParseIdentifierOrKeyword()
    {
        char current;

        _builder.Clear();

        while ((current = Peek()) != '\0' && current != ' ' && (char.IsLetterOrDigit(current) || current == '_'))
        {
            _builder.Append(current);
            _position++;
        }

        var text = _builder.ToString();
        var token = new SyntaxToken(IsKeyword(text) ? SyntaxType.Keyword : SyntaxType.Identifier, _position, text);

        return token;
    }

    private SyntaxToken ParseString()
    {
        char current;

        _builder.Clear();

        _position++;
        while (!((current = Peek()) == '"' && Peek(-1) != '\\') && current != '\0')
        {
            if (current == '\\')
            {
                _position++;

                current = Peek();

                switch (current)
                {
                    case '"': _builder.Append('\"'); break;
                    case 'n': _builder.Append('\n'); break;
                    case '\\': _builder.Append('\\'); break;
                    default: _diagnostics.Add(Diagnostic.Create(_path, CreateBadToken(current.ToString()), "Invalid escape sequence")); break;
                }

                _position++;
            }
            else
            {
                _builder.Append(current);
                _position++;
            }
        }

        _position++;
        return new(SyntaxType.String, _position - 1, _builder.ToString());
    }

    private SyntaxToken ParseNumber()
    {
        var style = NumberStyle.Decimal;

        _builder.Clear();

        while (true)
        {
            var current = Peek();
            var lookAhead = Peek(1);

            switch (current)
            {
                case '0' when lookAhead is 'x':
                {
                    _position += 2;
                    style = NumberStyle.Hexadecimal;
                    continue;
                }
                case '0' when lookAhead is 'b':
                {
                    _position += 2;
                    style = NumberStyle.Binary;
                    continue;
                }
            }

            switch (style)
            {
                case NumberStyle.Decimal:
                {
                    if ((char.IsDigit(current) || current is '.') && current is not '\0')
                    {
                        _builder.Append(current);
                        _position++;
                        continue;
                    }

                    if (char.IsLetter(current) && char.IsLetter(lookAhead))
                    {
                        _builder.Append(current);
                        _builder.Append(lookAhead);
                        _position += 2;
                    }

                    break;
                }
                case NumberStyle.Hexadecimal:
                {
                    if (char.IsDigit(current) || char.ToLowerInvariant(current) is 'a' or 'b' or 'c' or 'd' or 'e' or 'f')
                    {
                        _builder.Append(current);
                        _position++;
                        continue;
                    }

                    break;
                }
                case NumberStyle.Binary:
                {
                    if (current is '0' or '1')
                    {
                        _builder.Append(current);
                        _position++;
                        continue;
                    }

                    break;
                }
            }

            break;
        }

        return new(style switch
        {
            NumberStyle.Decimal => SyntaxType.Decimal,
            NumberStyle.Hexadecimal => SyntaxType.Hexadecimal,
            NumberStyle.Binary => SyntaxType.Binary
        }, _position - 1, _builder.ToString());
    }

    private SyntaxToken CreateBadToken(string text) => new(SyntaxType.BadToken, _position, text);

    private static bool IsKeyword(string text) => text
        is "return"
        or "if" or "else"
        or "continue" or "break" or "for" or "while"
        or "var" or "fld" or "mut"
        or "using" or "import" or "export"
        or "alloc" or "dealloc" or "stackalloc" or "alloca"
        or "fn" or "mod" or "stc"
        or "sizeof"
        or "dyn" or "pub" or "native" or "ext" or "impl"
        or "true" or "false";
}
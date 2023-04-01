using Materisk.Lex;
using Materisk.Parse.Nodes;
using Materisk.Parse.Nodes.Branch;
using Materisk.Parse.Nodes.Definition;
using Materisk.Parse.Nodes.Identifier;
using Materisk.Parse.Nodes.Literal;
using Materisk.Parse.Nodes.Misc;
using Materisk.Parse.Nodes.Operator;

namespace Materisk.Parse;

public class Parser
{
    private readonly List<SyntaxToken> _tokens;
    private int _position;

    public Parser(List<SyntaxToken> tokens)
    {
        _tokens = tokens;
    }

    public SyntaxNode Parse()
    {
        var nodes = new List<SyntaxNode>();

        while (Peek().Type != SyntaxType.Eof)
            nodes.Add(ParseStatement());

        return new BlockNode(nodes);
    }

    private SyntaxNode ParseScopedStatements()
    {
        MatchToken(SyntaxType.LBraces);

        SyntaxToken current;

        var nodes = new List<SyntaxNode>();

        while ((current = Peek()).Type != SyntaxType.RBraces)
        {
            if (current.Type == SyntaxType.Eof)
                throw new Exception($"Unclosed block at position {current.Position}");

            nodes.Add(ParseStatement());
        }

        MatchToken(SyntaxType.RBraces);

        return new BlockNode(nodes);
    }

    private SyntaxNode ParseStatement()
    {
        switch (Peek())
        {
            case { Type: SyntaxType.Keyword, Text: "return" } when Peek(1).Type == SyntaxType.Semicolon:
            {
                _position += 2;
                var ret = new ReturnNode();
                MatchToken(SyntaxType.Semicolon);
                return ret;
            }
            case { Type: SyntaxType.Keyword, Text: "return" }:
            {
                _position++;
                var ret = new ReturnNode(ParseExpression(null, null));
                MatchToken(SyntaxType.Semicolon);
                return ret;
            }
            case { Type: SyntaxType.Keyword, Text: "continue" }:
            {
                _position++;
                MatchToken(SyntaxType.Semicolon);
                return new ContinueNode();
            }
            case { Type: SyntaxType.Keyword, Text: "break" }:
            {
                _position++;
                MatchToken(SyntaxType.Semicolon);
                return new BreakNode();
            }
            case { Type: SyntaxType.Keyword, Text: "import" }:
            {
                _position++;

                var path = MatchToken(SyntaxType.String);
                MatchToken(SyntaxType.Semicolon);

                if (!File.Exists(path.Text))
                    throw new Exception($"Failed to import \"{path.Text}\": File not found");

                var lexer = new Lexer(File.ReadAllText(path.Text));
                var lexedTokens = lexer.Lex();

                var parser = new Parser(lexedTokens);
                return parser.Parse();
            }
            case { Type: SyntaxType.Keyword, Text: "fn" }:
            {
                return ParseFunctionDefinition();
            }
            case { Type: SyntaxType.Keyword, Text: "mod" }:
            {
                return ParseModuleDefinition();
            }
        }

        var exprNode = ParseExpression(null, null);
        MatchToken(SyntaxType.Semicolon);

        return exprNode;
    }

    private SyntaxNode ParseModuleDefinition()
    {
        MatchKeyword("mod");

        var isPublic = false;
        var isStatic = true;

        if (Peek() is { Type: SyntaxType.Keyword, Text: "pub" })
        {
            _position++;
            isPublic = true;
        }

        if (Peek() is { Type: SyntaxType.Keyword, Text: "dyn" })
        {
            _position++;
            isStatic = false;
        }

        var className = MatchToken(SyntaxType.Identifier);

        MatchToken(SyntaxType.LBraces);
        var body = ParseModuleBody(className, isStatic);
        MatchToken(SyntaxType.RBraces);

        return new ModuleDefinitionNode(className, body, isPublic);
    }

    private List<SyntaxNode> ParseModuleBody(SyntaxToken moduleName, bool isStatic)
    {
        var nodes = new List<SyntaxNode>();

        while (Peek() is { Type: SyntaxType.Keyword, Text: "fld" })
        {
            _position++;

            var isPublic = false;

            if (Peek() is { Type: SyntaxType.Keyword, Text: "pub" })
            {
                _position++;
                isPublic = true;
            }

            var nameToken = MatchToken(SyntaxType.Identifier);
            var type = MatchToken(SyntaxType.Identifier);

            if (Peek().Type == SyntaxType.Equals)
                throw new InvalidOperationException("Can not initialize a field directly!");

            MatchToken(SyntaxType.Semicolon);
            nodes.Add(new ModuleFieldDefinitionNode(isPublic, isStatic, nameToken, type));
        }

        while (Peek() is { Type: SyntaxType.Keyword, Text: "fn" })
        {
            _position++;

            var isNative = false;
            var isPublic = false;

            if (Peek() is { Type: SyntaxType.Keyword, Text: "native" })
            {
                _position++;
                isNative = true;
            }

            if (Peek() is { Type: SyntaxType.Keyword, Text: "pub" })
            {
                _position++;
                isPublic = true;
            }

            var name = MatchToken(SyntaxType.Identifier);
            var args = ParseFunctionArgs();
            var returnType = MatchToken(SyntaxType.Identifier);
            var body = ParseScopedStatements();

            nodes.Add(new ModuleFunctionDefinitionNode(moduleName, name, args, returnType, body, isStatic, isPublic, isNative));
        }

        return nodes;
    }

    private SyntaxNode ParseExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        if (Peek() is { Type: SyntaxType.Keyword, Text: "var" })
        {
            _position++;

            var ident = MatchToken(SyntaxType.Identifier);

            var current = Peek();
            var mutable = false;
            if (current.Type is SyntaxType.Keyword)
            {
                _position++;
                mutable = current.Text is "mut";
            }

            var type = MatchToken(SyntaxType.Identifier);

            current = Peek();

            SyntaxToken? secondType = null;

            if (current.Type is SyntaxType.Identifier)
                secondType = MatchToken(SyntaxType.Identifier);
            else if (current.Type is not SyntaxType.Equals)
                throw new InvalidOperationException("Variable initialization needs an expression!");

            _position++;
            var expr = ParseExpression(type, secondType);
            return new InitVariableNode(mutable, ident, type, secondType, expr);
        }

        if (Peek().Type == SyntaxType.Identifier && Peek(1).Type
                is SyntaxType.Equals
                or SyntaxType.PlusEquals or SyntaxType.PlusPlus
                or SyntaxType.MinusEquals or SyntaxType.MinusMinus
                or SyntaxType.MulEquals
                or SyntaxType.DivEquals
                or SyntaxType.ModEquals
                )
        {
            var ident = MatchToken(SyntaxType.Identifier);
            var current = Peek();

            switch (current.Type)
            {
                case SyntaxType.PlusEquals:
                case SyntaxType.MinusEquals:
                case SyntaxType.MulEquals:
                case SyntaxType.DivEquals:
                case SyntaxType.ModEquals:
                {
                    var identifier = new IdentifierNode(ident);
                    var operatorToken = MatchToken(current.Type);
                    var expression = ParseExpression(typeToken, secondTypeToken);
                    return new AssignExpressionNode(ident, new BinaryExpressionNode(identifier, operatorToken, expression));
                }
                case SyntaxType.PlusPlus:
                case SyntaxType.MinusMinus:
                {
                    var identifier = new IdentifierNode(ident);
                    var operatorToken = MatchToken(current.Type);
                    var expression = new IntLiteralNode(new SyntaxToken(SyntaxType.Number, -1, "1"));
                    return new AssignExpressionNode(ident, new BinaryExpressionNode(identifier, operatorToken, expression));
                }
            }

            MatchToken(SyntaxType.Equals);
            var expr = ParseExpression(typeToken, secondTypeToken);
            return new AssignExpressionNode(ident, expr);
        }

        return ParseCompExpression(typeToken, secondTypeToken);
    }

    private SyntaxNode ParseCompExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        var current = Peek();

        if (current.Type == SyntaxType.Bang)
        {
            _position++;
            return new UnaryExpressionNode(current, ParseCompExpression(typeToken, secondTypeToken));
        }

        return BinaryOperation(() => ParseArithmeticExpression(typeToken, secondTypeToken),
            new[]
            {
                SyntaxType.EqualsEquals, SyntaxType.LessThan, SyntaxType.LessThanEqu, SyntaxType.GreaterThan, SyntaxType.GreaterThanEqu
            });
    }

    private SyntaxNode ParseArithmeticExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        return BinaryOperation(() => ParseTermExpression(typeToken, secondTypeToken), new[] { SyntaxType.Plus, SyntaxType.Minus });
    }

    private SyntaxNode ParseTermExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        return BinaryOperation(() => ParseFactorExpression(typeToken, secondTypeToken), new[] { SyntaxType.Mul, SyntaxType.Div, SyntaxType.Mod });
    }

    private SyntaxNode ParseFactorExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        var current = Peek();

        if (current.Type is SyntaxType.Plus or SyntaxType.Minus or SyntaxType.Bang)
        {
            _position++;
            var factor = ParseFactorExpression(typeToken, secondTypeToken);
            return new UnaryExpressionNode(current, factor);
        }

        return ParsePowerExpression(typeToken, secondTypeToken);
    }

    private SyntaxNode ParsePowerExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        return BinaryOperation(() => ParseDotExpression(typeToken, secondTypeToken), new[] { SyntaxType.Pow }, () => ParseFactorExpression(typeToken, secondTypeToken));
    }

    private SyntaxNode ParseDotExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        var callNode = ParseCallExpression(typeToken, secondTypeToken);
        var accessStack = new DotNode(callNode);

        if (Peek().Type is SyntaxType.Dot)
            while (Peek().Type is SyntaxType.Dot)
            {
                _position++;

                if (Peek().Type is SyntaxType.Identifier)
                    if (Peek(1).Type is SyntaxType.Equals)
                    {
                        var ident = MatchToken(SyntaxType.Identifier);
                        MatchToken(SyntaxType.Equals);
                        var expr = ParseExpression(typeToken, secondTypeToken);

                        accessStack.NextNodes.Add(new AssignExpressionNode(ident, expr));
                    }
                    else
                    {
                        var n = ParseCallExpression(typeToken, secondTypeToken);
                        accessStack.NextNodes.Add(n);
                    }
            }
        else
            return callNode;

        return accessStack;
    }

    private SyntaxNode ParseCallExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        var atomNode = ParseCastExpression(typeToken, secondTypeToken);

        if (Peek().Type is SyntaxType.LParen)
        {
            _position++;

            var argumentNodes = new List<SyntaxNode>();

            if (Peek().Type is not SyntaxType.RParen)
            {
                argumentNodes.Add(ParseExpression(typeToken, MatchToken(SyntaxType.Identifier)));

                while (Peek().Type is SyntaxType.Comma)
                {
                    _position++;
                    argumentNodes.Add(ParseExpression(typeToken, MatchToken(SyntaxType.Identifier)));
                }

                MatchToken(SyntaxType.RParen);
            } else _position++;

            return new CallNode(atomNode, argumentNodes);
        }

        return atomNode;
    }

    private SyntaxNode ParseCastExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        if (Peek().Type is SyntaxType.LParen)
        {
            MatchToken(SyntaxType.LParen);
            var ident = MatchToken(SyntaxType.Identifier);

            if (ident.Text is not "int" and not "float" and not "byte")
                throw new Exception($"Can not cast to: {ident.Text}");

            MatchToken(SyntaxType.RParen);

            var node = ParseCastExpression(typeToken, secondTypeToken);
            return new CastNode(ident, node);
        }

        return ParseAtomExpression(typeToken, secondTypeToken);
    }

    private SyntaxNode ParseAtomExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        var current = Peek();

        switch (current.Type)
        {
            case SyntaxType.Number:
            {
                _position++;

                if (typeToken?.Text is "ptr")
                    return new IntLiteralNode(current); // TODO: UInt

                if (typeToken is null && secondTypeToken is null)
                    return new IntLiteralNode(current);

                return (secondTypeToken is null ? typeToken.Text : secondTypeToken.Text) switch
                {
                    "int" => new IntLiteralNode(current),
                    "float" => new FloatLiteralNode(current),
                    "byte" => new ByteLiteralNode(current),
                    _ => throw new NotImplementedException($"Unimplemented number type: {secondTypeToken.Text}")
                };
            }
            case SyntaxType.String:
            {
                _position++;
                return new StringLiteralNode(current);
            }
            case SyntaxType.Identifier when Peek(1).Type is SyntaxType.LSqBracket:
            {
                return ParseIndexExpression(typeToken, secondTypeToken);
            }
            case SyntaxType.Identifier:
            {
                _position++;
                return new IdentifierNode(current);
            }
            case SyntaxType.LParen:
            {
                _position++;
                var expr = ParseExpression(typeToken, secondTypeToken);
                MatchToken(SyntaxType.RParen);
                return expr;
            }
            case SyntaxType.LSqBracket:
            {
                return ParseArrayExpression(typeToken, secondTypeToken);
            }
            case SyntaxType.Keyword when Peek().Text == "if":
            {
                return ParseIfExpression(typeToken, secondTypeToken);
            }
        }

        return current.Type switch
        {
            SyntaxType.Keyword when current.Text == "for" => ParseForExpression(typeToken, secondTypeToken),
            SyntaxType.Keyword when current.Text == "while" => ParseWhileExpression(typeToken, secondTypeToken),
            SyntaxType.Keyword when current.Text == "fld" => ParseFieldExpression(),
            SyntaxType.Keyword when current.Text == "new" => ParseInstantiateExpression(typeToken, secondTypeToken),
            _ => throw new Exception($"Unexpected token {Peek().Type} at position {Peek().Position} in atom expression!")
        };
    }

    private SyntaxNode ParseIndexExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        var ident = MatchToken(SyntaxType.Identifier);
        MatchToken(SyntaxType.LSqBracket);
        var expr = ParseExpression(typeToken, secondTypeToken);
        MatchToken(SyntaxType.RSqBracket);

        SyntaxNode? setNode = null;

        if (Peek().Type is SyntaxType.Equals)
        {
            _position++;
            setNode = ParseExpression(typeToken, secondTypeToken);
        }

        return new IndexNode(new IdentifierNode(ident), expr, setNode);
    }

    private SyntaxNode ParseArrayExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        if (secondTypeToken is null)
            throw new InvalidOperationException("Invalid array type!");

        MatchToken(SyntaxType.LSqBracket);

        if (Peek().Type == SyntaxType.RSqBracket)
            throw new InvalidOperationException("Array length not specified!");

        var expr = ParseExpression(typeToken, secondTypeToken);
        MatchToken(SyntaxType.RSqBracket);

        return new ArrayNode(secondTypeToken, expr);
    }

    private SyntaxNode ParseIfExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        MatchKeyword("if");

        MatchToken(SyntaxType.LParen);
        var conditionNode = ParseExpression(typeToken, secondTypeToken);
        MatchToken(SyntaxType.RParen);

        var blockNode = ParseScopedStatements();

        if (Peek() is { Type: SyntaxType.Keyword, Text: "else" })
        {
            _position++;
            var elseBlockNode = ParseScopedStatements();

            return new IfNode(conditionNode, blockNode, elseBlockNode);
        }

        return new IfNode(conditionNode, blockNode);
    }

    private SyntaxNode ParseForExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        MatchKeyword("for");

        MatchToken(SyntaxType.LParen);
        var initialExpressionNode = ParseExpression(typeToken, secondTypeToken);
        MatchToken(SyntaxType.Semicolon);
        var condNode = ParseExpression(typeToken, secondTypeToken);
        MatchToken(SyntaxType.Semicolon);
        var stepNode = ParseExpression(typeToken, secondTypeToken);
        MatchToken(SyntaxType.RParen);
        var block = ParseScopedStatements();

        return new ForNode(initialExpressionNode, condNode, stepNode, block);
    }

    private SyntaxNode ParseWhileExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        MatchKeyword("while");

        MatchToken(SyntaxType.LParen);
        var condNode = ParseExpression(typeToken, secondTypeToken);
        MatchToken(SyntaxType.RParen);
        var block = ParseScopedStatements();

        return new WhileNode(condNode, block);
    }

    private SyntaxNode ParseFunctionDefinition()
    {
        MatchKeyword("fn");

        var isNative = false;
        var isPublic = false;

        if (Peek() is { Type: SyntaxType.Keyword, Text: "native" })
        {
            _position++;
            isNative = true;
        }

        if (Peek() is { Type: SyntaxType.Keyword, Text: "pub" })
        {
            _position++;
            isPublic = true;
        }

        var nameToken = MatchToken(SyntaxType.Identifier);
        var args = ParseFunctionArgs();
        var returnType = MatchToken(SyntaxType.Identifier);
        var block = ParseScopedStatements();

        return new FunctionDefinitionNode(nameToken, args, returnType, block, isPublic, isNative);
    }

    private SyntaxNode ParseFieldExpression()
    {
        MatchKeyword("fld");

        var isPublic = false;

        if (Peek() is { Type: SyntaxType.Keyword, Text: "pub" })
        {
            _position++;
            isPublic = true;
        }

        var nameToken = MatchToken(SyntaxType.Identifier);
        var type = MatchToken(SyntaxType.Identifier);

        if (Peek().Type == SyntaxType.Equals)
            throw new InvalidOperationException("Can not initialize a field directly!");

        return new FieldDefinitionNode(isPublic, nameToken, type);
    }

    private SyntaxNode ParseInstantiateExpression(SyntaxToken? typeToken, SyntaxToken? secondTypeToken)
    {
        _position++;
        var ident = MatchToken(SyntaxType.Identifier);

        var argumentNodes = new List<SyntaxNode>();

        if (Peek().Type is SyntaxType.LParen)
        {
            _position++;

            if (Peek().Type is not SyntaxType.RParen)
            {
                argumentNodes.Add(ParseExpression(typeToken, secondTypeToken));

                while (Peek().Type is SyntaxType.Comma)
                {
                    _position++;

                    argumentNodes.Add(ParseExpression(typeToken, secondTypeToken));
                }

                MatchToken(SyntaxType.RParen);
            } else _position++;
        }

        return new InstantiateNode(ident, argumentNodes);
    }

    private Dictionary<SyntaxToken, SyntaxToken> ParseFunctionArgs()
    {
        MatchToken(SyntaxType.LParen);

        var args = new Dictionary<SyntaxToken, SyntaxToken>();

        if (Peek().Type is not SyntaxType.RParen)
        {
            var type = MatchToken(SyntaxType.Identifier);
            var ident = MatchToken(SyntaxType.Identifier);

            args.Add(type, ident);

            while (Peek().Type == SyntaxType.Comma)
            {
                _position++;
                type = MatchToken(SyntaxType.Identifier);
                ident = MatchToken(SyntaxType.Identifier);
                args.Add(type, ident);
            }
        }

        MatchToken(SyntaxType.RParen);
        return args;
    }

    private SyntaxNode BinaryOperation(Func<SyntaxNode> leftParse, SyntaxType[] allowedTypes, Func<SyntaxNode>? rightParse = null)
    {
        SyntaxToken current;
        
        var left = leftParse();

        while (allowedTypes.Contains((current = Peek()).Type))
        {
            _position++;
            var right = (rightParse ?? leftParse)();

            left = new BinaryExpressionNode(left, current, right);
        }

        return left;
    }

    private SyntaxToken Peek(int off = 0)
    {
        var offset = _position + off;
        if (offset >= 0 && offset < _tokens.Count)
            return _tokens[offset];
        return new(SyntaxType.BadToken, 0, string.Empty);
    }

    private SyntaxToken MatchToken(SyntaxType type)
    {
        var current = Peek();

        if (current.Type == type)
        {
            _position++;
            return current;
        }

        throw new Exception("Unexpected token " + current.Type + "; expected " + type);
    }

    private SyntaxToken MatchKeyword(string value)
    {
        var current = Peek();

        if (current.Type == SyntaxType.Keyword && current.Text == value)
        {
            _position++;
            return current;
        }

        throw new Exception("Unexpected token " + current.Type + "; expected Keyword with value " + value);
    }
}
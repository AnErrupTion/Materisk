using System.Globalization;
using Materisk.Lex;
using Materisk.Parse.Nodes;
using Materisk.Parse.Nodes.Branch;
using Materisk.Parse.Nodes.Definition;
using Materisk.Parse.Nodes.Identifier;
using Materisk.Parse.Nodes.Literal;
using Materisk.Parse.Nodes.Misc;
using Materisk.Parse.Nodes.Operator;
using Materisk.Utils;

namespace Materisk.Parse;

public sealed class Parser
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

    private BlockNode ParseScopedStatements(bool checkForReturn = false)
    {
        MatchToken(SyntaxType.LBraces);

        SyntaxToken current;

        var nodes = new List<SyntaxNode>();
        var hasReturn = false;

        while ((current = Peek()).Type != SyntaxType.RBraces)
        {
            if (current.Type == SyntaxType.Eof)
                throw new InvalidOperationException($"Unclosed block at position: {current.Position}");

            var statement = ParseStatement(false);
            if (checkForReturn && statement is ReturnNode)
                hasReturn = true;

            nodes.Add(statement);
        }

        MatchToken(SyntaxType.RBraces);

        if (checkForReturn && !hasReturn)
            nodes.Add(new ReturnNode());

        return new BlockNode(nodes);
    }

    private SyntaxNode ParseStatement(bool semiColon = true)
    {
        switch (Peek())
        {
            case { Type: SyntaxType.Keyword, Text: "return" } when Peek(1).Type == SyntaxType.Semicolon:
            {
                _position += 2;
                var ret = new ReturnNode();
                return ret;
            }
            case { Type: SyntaxType.Keyword, Text: "return" }:
            {
                _position++;
                var ret = new ReturnNode(ParseExpression(null!));
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
            case { Type: SyntaxType.Keyword, Text: "using" }:
            {
                _position++;

                var path = MatchToken(SyntaxType.String).Text;
                MatchToken(SyntaxType.Semicolon);

                if (!File.Exists(path))
                    throw new InvalidOperationException($"Failed to import \"{path}\": File not found");

                var lexer = new Lexer(File.ReadAllText(path)); 
                var lexedTokens = lexer.Lex(); 
                var parser = new Parser(lexedTokens); 

                return new UsingDefinitionNode(path, parser.Parse());
            }
            case { Type: SyntaxType.Keyword, Text: "fld" }:
            {
                return ParseFieldExpression(true);
            }
            case { Type: SyntaxType.Keyword, Text: "fn" }:
            {
                return ParseFunctionDefinition(true);
            }
            case { Type: SyntaxType.Keyword, Text: "mod" }:
            {
                return ParseModuleDefinition();
            }
        }

        var exprNode = ParseExpression(null!);

        if (semiColon)
            MatchToken(SyntaxType.Semicolon);
        else if (Peek().Type is SyntaxType.Semicolon)
            _position++;

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

        var moduleName = MatchToken(SyntaxType.Identifier);

        MatchToken(SyntaxType.LBraces);
        var body = ParseModuleBody(isStatic);
        MatchToken(SyntaxType.RBraces);

        return new ModuleDefinitionNode(moduleName.Text, body, isPublic);
    }

    private List<SyntaxNode> ParseModuleBody(bool isStatic)
    {
        var nodes = new List<SyntaxNode>();

        while (Peek() is { Type: SyntaxType.Keyword, Text: "fld" })
            nodes.Add(ParseFieldExpression(isStatic));

        while (Peek() is { Type: SyntaxType.Keyword, Text: "fn" })
            nodes.Add(ParseFunctionDefinition(isStatic));

        return nodes;
    }

    private SyntaxNode ParseExpression(SyntaxToken? secondTypeToken)
    {
        var current = Peek();

        if (current is { Type: SyntaxType.Keyword, Text: "var" })
        {
            _position++;

            var ident = MatchToken(SyntaxType.Identifier);

            if (string.IsNullOrEmpty(ident.Text))
                throw new InvalidOperationException("Can not assign to a non-existent identifier!");

            var mutable = false;

            current = Peek();

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
                throw new InvalidOperationException($"Variable initialization needs an expression: {ident.Text}");

            _position++;
            var expr = ParseExpression(secondType);
            return new InitVariableNode(mutable, ident.Text, type.Text, secondType is null ? string.Empty : secondType.Text, expr);
        }

        if (current.Type == SyntaxType.Identifier && Peek(1).Type
                is SyntaxType.Equals
                or SyntaxType.PlusEquals or SyntaxType.PlusPlus
                or SyntaxType.MinusEquals or SyntaxType.MinusMinus
                or SyntaxType.MulEquals
                or SyntaxType.DivEquals
                or SyntaxType.ModEquals
                )
        {
            var ident = MatchToken(SyntaxType.Identifier);

            if (string.IsNullOrEmpty(ident.Text))
                throw new InvalidOperationException("Can not assign to a non-existent identifier!");

            current = Peek();

            switch (current.Type)
            {
                case SyntaxType.PlusEquals:
                case SyntaxType.MinusEquals:
                case SyntaxType.MulEquals:
                case SyntaxType.DivEquals:
                case SyntaxType.ModEquals:
                {
                    var identifier = new IdentifierNode(ident.Text);
                    var operatorToken = MatchToken(current.Type);
                    var expression = ParseExpression(secondTypeToken);
                    return new AssignExpressionNode(ident.Text, new BinaryExpressionNode(identifier, operatorToken.Text, expression));
                }
                case SyntaxType.PlusPlus:
                case SyntaxType.MinusMinus:
                {
                    var identifier = new IdentifierNode(ident.Text);
                    var operatorToken = MatchToken(current.Type);
                    var expression = new UIntLiteralNode(1);
                    return new AssignExpressionNode(ident.Text, new BinaryExpressionNode(identifier, operatorToken.Text, expression));
                }
            }

            MatchToken(SyntaxType.Equals);
            var expr = ParseExpression(secondTypeToken);
            return new AssignExpressionNode(ident.Text, expr);
        }

        return ParseBinaryExpression(secondTypeToken);
    }

    private SyntaxNode ParseBinaryExpression(SyntaxToken? secondTypeToken, int parentPrecedence = 0)
    {
        var current = Peek();
        var unaryPrecedence = current.Type.GetUnaryOperatorPrecedence();

        SyntaxNode left;
        if (unaryPrecedence is not 0 && unaryPrecedence >= parentPrecedence)
        {
            _position++;
            var expr = ParseBinaryExpression(secondTypeToken, unaryPrecedence);
            left = new UnaryExpressionNode(current.Text, expr);
        } else left = ParseParenthesizedExpression(secondTypeToken);

        for (;;)
        {
            current = Peek();

            var binaryPrecedence = current.Type.GetBinaryOperatorPrecedence();

            if (binaryPrecedence is 0 || binaryPrecedence <= parentPrecedence)
                break;

            _position++;
            left = new BinaryExpressionNode(left, current.Text, ParseBinaryExpression(secondTypeToken, binaryPrecedence));
        }

        return left;
    }

    private SyntaxNode ParseParenthesizedExpression(SyntaxToken? secondTypeToken)
    {
        if (Peek().Type is SyntaxType.LParen)
        {
            _position++;
            var expr = ParseExpression(secondTypeToken);
            MatchToken(SyntaxType.RParen);
            return expr;
        }

        return ParseDotExpression(secondTypeToken);
    }

    private SyntaxNode ParseDotExpression(SyntaxToken? secondTypeToken)
    {
        var callNode = ParseCallExpression(secondTypeToken);
        var accessStack = new DotNode(callNode);

        if (Peek().Type is SyntaxType.Dot)
            while (Peek().Type is SyntaxType.Dot)
            {
                _position++;

                if (Peek().Type is SyntaxType.Identifier)
                    if (Peek(1).Type is SyntaxType.Equals)
                    {
                        var ident = MatchToken(SyntaxType.Identifier);

                        if (string.IsNullOrEmpty(ident.Text))
                            throw new InvalidOperationException("Can not assign to a non-existent identifier!");

                        MatchToken(SyntaxType.Equals);
                        var expr = ParseExpression(secondTypeToken);

                        accessStack.NextNodes.Add(new AssignExpressionNode(ident.Text, expr));
                    }
                    else
                    {
                        var n = ParseCallExpression(secondTypeToken);
                        accessStack.NextNodes.Add(n);
                    }
            }
        else
            return callNode;

        return accessStack;
    }

    private SyntaxNode ParseCallExpression(SyntaxToken? secondTypeToken)
    {
        var atomNode = ParseCastExpression(secondTypeToken);

        if (Peek().Type is SyntaxType.LParen)
        {
            _position++;

            var argumentNodes = new List<SyntaxNode>();

            if (Peek().Type is not SyntaxType.RParen)
            {
                argumentNodes.Add(ParseExpression(secondTypeToken));

                while (Peek().Type is SyntaxType.Comma)
                {
                    _position++;
                    argumentNodes.Add(ParseExpression(secondTypeToken));
                }

                MatchToken(SyntaxType.RParen);
            } else _position++;

            return new CallNode(atomNode, argumentNodes);
        }

        return atomNode;
    }

    private SyntaxNode ParseCastExpression(SyntaxToken? secondTypeToken)
    {
        if (Peek().Type is SyntaxType.LParen)
        {
            MatchToken(SyntaxType.LParen);
            var typeToken = MatchToken(SyntaxType.Identifier);

            SyntaxToken? nextTypeToken = null;

            var current = Peek();
            if (current.Type is SyntaxType.Identifier)
            {
                _position++;
                nextTypeToken = current;
            }

            MatchToken(SyntaxType.RParen);

            var node = ParseCastExpression(secondTypeToken);
            return new CastNode(typeToken.Text, nextTypeToken is null ? string.Empty : nextTypeToken.Text, node);
        }

        return ParseAtomExpression(secondTypeToken);
    }

    private SyntaxNode ParseAtomExpression(SyntaxToken? secondTypeToken)
    {
        var current = Peek();

        switch (current.Type)
        {
            case SyntaxType.Number:
            {
                _position++;

                var numberText = current.Text;

                if (numberText.EndsWith("SB") && sbyte.TryParse(numberText[..^2], CultureInfo.InvariantCulture, out var sbyteNumber))
                    return new SByteLiteralNode(sbyteNumber);

                if (numberText.EndsWith("UB") && byte.TryParse(numberText[..^2], CultureInfo.InvariantCulture, out var byteNumber))
                    return new ByteLiteralNode(byteNumber);

                if (numberText.EndsWith("SS") && short.TryParse(numberText[..^2], CultureInfo.InvariantCulture, out var shortNumber))
                    return new ShortLiteralNode(shortNumber);

                if (numberText.EndsWith("US") && ushort.TryParse(numberText[..^2], CultureInfo.InvariantCulture, out var ushortNumber))
                    return new UShortLiteralNode(ushortNumber);

                if (numberText.EndsWith("SI") && int.TryParse(numberText[..^2], CultureInfo.InvariantCulture, out var intNumberExplicit))
                    return new IntLiteralNode(intNumberExplicit);

                if (numberText.EndsWith("UI") && uint.TryParse(numberText[..^2], CultureInfo.InvariantCulture, out var uintNumberExplicit))
                    return new UIntLiteralNode(uintNumberExplicit);

                if (numberText.EndsWith("SL") && long.TryParse(numberText[..^2], CultureInfo.InvariantCulture, out var longNumberExplicit))
                    return new LongLiteralNode(longNumberExplicit);

                if (numberText.EndsWith("UL") && ulong.TryParse(numberText[..^2], CultureInfo.InvariantCulture, out var ulongNumberExplicit))
                    return new ULongLiteralNode(ulongNumberExplicit);

                if (uint.TryParse(numberText, CultureInfo.InvariantCulture, out var uintNumber))
                    return new UIntLiteralNode(uintNumber);

                if (ulong.TryParse(numberText, CultureInfo.InvariantCulture, out var ulongNumber))
                    return new ULongLiteralNode(ulongNumber);

                if (int.TryParse(numberText, CultureInfo.InvariantCulture, out var intNumber))
                    return new IntLiteralNode(intNumber);

                if (long.TryParse(numberText, CultureInfo.InvariantCulture, out var longNumber))
                    return new LongLiteralNode(longNumber);

                if (float.TryParse(numberText, CultureInfo.InvariantCulture, out var floatNumber))
                    return new FloatLiteralNode(floatNumber);

                if (double.TryParse(numberText, CultureInfo.InvariantCulture, out var doubleNumber))
                    return new DoubleLiteralNode(doubleNumber);

                throw new InvalidOperationException($"Unable to parse number: {numberText}");
            }
            case SyntaxType.String:
            {
                _position++;
                return new StringLiteralNode(current.Text);
            }
            case SyntaxType.Identifier when Peek(1).Type is SyntaxType.LSqBracket:
            {
                return ParseIndexExpression(secondTypeToken);
            }
            case SyntaxType.Identifier:
            {
                _position++;
                return new IdentifierNode(current.Text);
            }
            case SyntaxType.LParen:
            {
                _position++;
                var expr = ParseExpression(secondTypeToken);
                MatchToken(SyntaxType.RParen);
                return expr;
            }
            case SyntaxType.LSqBracket:
            {
                return ParseArrayExpression(secondTypeToken);
            }
            case SyntaxType.Keyword when current.Text is "if":
            {
                return ParseIfExpression(secondTypeToken);
            }
            case SyntaxType.Keyword when current.Text is "true":
            {
                _position++;
                return new BoolLiteralNode(true);
            }
            case SyntaxType.Keyword when current.Text is "false":
            {
                _position++;
                return new BoolLiteralNode(false);
            }
        }

        return current.Type switch
        {
            SyntaxType.Keyword when current.Text == "for" => ParseForExpression(secondTypeToken),
            SyntaxType.Keyword when current.Text == "while" => ParseWhileExpression(secondTypeToken),
            SyntaxType.Keyword when current.Text == "new" => ParseInstantiateExpression(secondTypeToken),
            _ => throw new InvalidOperationException($"Unexpected token {Peek().Type} at position {Peek().Position} in atom expression!")
        };
    }

    private SyntaxNode ParseIndexExpression(SyntaxToken? secondTypeToken)
    {
        var ident = MatchToken(SyntaxType.Identifier);
        MatchToken(SyntaxType.LSqBracket);
        var expr = ParseExpression(secondTypeToken);
        MatchToken(SyntaxType.RSqBracket);

        SyntaxNode? setNode = null;

        if (Peek().Type is SyntaxType.Equals)
        {
            _position++;
            setNode = ParseExpression(secondTypeToken);
        }

        return new IndexNode(new IdentifierNode(ident.Text), expr, setNode);
    }

    private SyntaxNode ParseArrayExpression(SyntaxToken? secondTypeToken)
    {
        if (secondTypeToken is null)
            throw new InvalidOperationException("Invalid array type!");

        MatchToken(SyntaxType.LSqBracket);

        if (Peek().Type == SyntaxType.RSqBracket)
            throw new InvalidOperationException("Array length not specified!");

        var expr = ParseExpression(null!);
        MatchToken(SyntaxType.RSqBracket);

        return new ArrayNode(secondTypeToken.Text, expr);
    }

    private SyntaxNode ParseIfExpression(SyntaxToken? secondTypeToken)
    {
        MatchKeyword("if");

        MatchToken(SyntaxType.LParen);
        var conditionNode = ParseExpression(secondTypeToken);
        MatchToken(SyntaxType.RParen);

        var blockNode = ParseScopedStatements();

        if (Peek() is { Type: SyntaxType.Keyword, Text: "else" })
        {
            _position++;

            if (Peek() is { Type: SyntaxType.Keyword, Text: "if" })
                return new IfNode(conditionNode, blockNode, ParseIfExpression(secondTypeToken));

            return new IfNode(conditionNode, blockNode, ParseScopedStatements());
        }

        return new IfNode(conditionNode, blockNode);
    }

    private SyntaxNode ParseForExpression(SyntaxToken? secondTypeToken)
    {
        MatchKeyword("for");

        MatchToken(SyntaxType.LParen);
        var initialExpressionNode = ParseExpression(secondTypeToken);
        MatchToken(SyntaxType.Semicolon);
        var condNode = ParseExpression(secondTypeToken);
        MatchToken(SyntaxType.Semicolon);
        var stepNode = ParseExpression(secondTypeToken);
        MatchToken(SyntaxType.RParen);
        var block = ParseScopedStatements();

        return new ForNode(initialExpressionNode, condNode, stepNode, block);
    }

    private SyntaxNode ParseWhileExpression(SyntaxToken? secondTypeToken)
    {
        MatchKeyword("while");

        MatchToken(SyntaxType.LParen);
        var condNode = ParseExpression(secondTypeToken);
        MatchToken(SyntaxType.RParen);
        var block = ParseScopedStatements();

        return new WhileNode(condNode, block);
    }

    private SyntaxNode ParseFunctionDefinition(bool isStatic)
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

        SyntaxToken? secondReturnType = null;

        var current = Peek();
        if (current.Type is SyntaxType.Identifier)
        {
            _position++;
            secondReturnType = current;
        }

        var block = ParseScopedStatements(true);

        return new FunctionDefinitionNode(nameToken.Text, args, returnType.Text,
            secondReturnType is null ? string.Empty : secondReturnType.Text, block, isPublic, isStatic, isNative);
    }

    private SyntaxNode ParseFieldExpression(bool isStatic)
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

        SyntaxToken? secondType = null;

        var current = Peek();
        if (current.Type is SyntaxType.Identifier)
        {
            _position++;
            secondType = current;
        }

        if (Peek().Type == SyntaxType.Equals)
            throw new InvalidOperationException($"Can not initialize a field directly: {nameToken.Text}");

        return new FieldDefinitionNode(isPublic, isStatic, nameToken.Text, type.Text,
            secondType is null ? string.Empty : secondType.Text);
    }

    private SyntaxNode ParseInstantiateExpression(SyntaxToken? secondTypeToken)
    {
        _position++;
        var ident = MatchToken(SyntaxType.Identifier);

        var argumentNodes = new List<SyntaxNode>();

        if (Peek().Type is SyntaxType.LParen)
        {
            _position++;

            if (Peek().Type is not SyntaxType.RParen)
            {
                argumentNodes.Add(ParseExpression(secondTypeToken));

                while (Peek().Type is SyntaxType.Comma)
                {
                    _position++;

                    argumentNodes.Add(ParseExpression(secondTypeToken));
                }

                MatchToken(SyntaxType.RParen);
            } else _position++;
        }

        return new InstantiateNode(ident.Text, argumentNodes);
    }

    private List<MethodArgument> ParseFunctionArgs()
    {
        MatchToken(SyntaxType.LParen);

        var args = new List<MethodArgument>();

        if (Peek().Type is not SyntaxType.RParen)
        {
            var type = MatchToken(SyntaxType.Identifier);
            var ident = MatchToken(SyntaxType.Identifier);

            SyntaxToken? secondType = null;

            if (Peek().Type is SyntaxType.Identifier)
            {
                secondType = ident;
                ident = MatchToken(SyntaxType.Identifier);
            }

            args.Add(new(type.Text, secondType is null ? string.Empty : secondType.Text, ident.Text));

            while (Peek().Type == SyntaxType.Comma)
            {
                _position++;

                type = MatchToken(SyntaxType.Identifier);
                ident = MatchToken(SyntaxType.Identifier);

                if (Peek().Type is SyntaxType.Identifier)
                {
                    secondType = ident;
                    ident = MatchToken(SyntaxType.Identifier);
                }

                args.Add(new(type.Text, secondType is null ? string.Empty : secondType.Text, ident.Text));
            }
        }

        MatchToken(SyntaxType.RParen);
        return args;
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

        throw new InvalidOperationException($"Unexpected token \"{current.Type}\" at position {_position}, expected: {type} ");
    }

    private void MatchKeyword(string value)
    {
        var current = Peek();

        if (current.Type == SyntaxType.Keyword && current.Text == value)
        {
            _position++;
            return;
        }

        throw new InvalidOperationException($"Unexpected token \"{current.Type}\" at position {_position}, expected Keyword with value: {value} ");
    }
}
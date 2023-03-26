using Materisk.Lex;
using Materisk.Parse.Nodes;

namespace Materisk.Parse;

public class Parser {
    public SyntaxToken[] Tokens { get; }
    public int Position;

    public SyntaxToken Current => Peek();

    public SyntaxToken Peek(int off = 0) {
        if (Position + off >= Tokens.Length || Position + off < 0) return new(SyntaxType.BadToken, 0, string.Empty);
        return Tokens[Position + off];
    }

    public SyntaxToken MatchToken(SyntaxType type) {
        if(Current.Type == type) {
            Position++;
            return Peek(-1);
        }

        throw new Exception("Unexpected token " + Current.Type + "; expected " + type);
    }

    public SyntaxToken MatchTokenWithText(SyntaxType type, string text)
    {
        if (Current.Type == type && Current.Text == text) {
            Position++;
            return Peek(-1);
        }

        throw new Exception("Unexpected token " + Current.Type + "; expected " + type + " with text " + text);
    }

    public SyntaxToken MatchKeyword(string value) {
        if (Current.Type == SyntaxType.Keyword && Current.Text == value) {
            Position++;
            return Peek(-1);
        }

        throw new Exception("Unexpected token " + Current.Type + "; expected Keyword with value " + value);
    }

    public Parser(SyntaxToken[] tokens) {
        Tokens = tokens;
    }

    public SyntaxNode Parse() {
        return ParseStatements();
    }

    public SyntaxNode ParseStatements() {
        List<SyntaxNode> nodes = new();

        while(Current.Type != SyntaxType.Eof) {
            nodes.Add(ParseStatement());
        }

        return new BlockNode(nodes, false);
    }

    public SyntaxNode ParseScopedStatements() {
        MatchToken(SyntaxType.LBraces);
        List<SyntaxNode> nodes = new();

        while(Current.Type != SyntaxType.RBraces) {
            if (Current.Type == SyntaxType.Eof) throw new Exception("Unclosed block at " + Current.Position);

            nodes.Add(ParseStatement());
        }

        MatchToken(SyntaxType.RBraces);

        return new BlockNode(nodes);
    }

    public SyntaxNode ParseStatement()
    {
        if (Current is { Type: SyntaxType.Keyword, Text: "return" }) {
            if (Peek(1).Type == SyntaxType.Semicolon) {
                Position += 2;
                var ret = new ReturnNode();
                MatchToken(SyntaxType.Semicolon);
                return ret;
            } else {
                Position++;
                var ret = new ReturnNode(ParseExpression(null));
                MatchToken(SyntaxType.Semicolon);
                return ret;
            }
        }

        if (Current is { Type: SyntaxType.Keyword, Text: "continue" }) {
            Position++;
            MatchToken(SyntaxType.Semicolon);
            return new ContinueNode();
        }
        if (Current is { Type: SyntaxType.Keyword, Text: "break" }) {
            Position++;
            MatchToken(SyntaxType.Semicolon);
            return new BreakNode();
        }
        if (Current is { Type: SyntaxType.Keyword, Text: "import" }) {
            Position++;

            var path = MatchToken(SyntaxType.String);
            MatchToken(SyntaxType.Semicolon);

            return new ImportNode(path);
        }
        if (Current is { Type: SyntaxType.Keyword, Text: "fn" }) {
            return ParseFunctionDefinition();
        }
        if (Current is { Type: SyntaxType.Keyword, Text: "mod" }) {
            return ParseModuleDefinition();
        }
        var exprNode = ParseExpression(null);
        MatchToken(SyntaxType.Semicolon);

        return exprNode;
    }

    private SyntaxNode ParseModuleDefinition() {
        MatchKeyword("mod");

        var isPublic = false;
        var isStatic = true;

        if(Current is { Type: SyntaxType.Keyword, Text: "pub" }) {
            Position++;
            isPublic = true;
        }

        if(Current is { Type: SyntaxType.Keyword, Text: "dyn" }) {
            Position++;
            isStatic = false;
        }

        var className = MatchToken(SyntaxType.Identifier);

        MatchToken(SyntaxType.LBraces);
        var body = ParseModuleBody(className, isStatic);
        MatchToken(SyntaxType.RBraces);

        return new ModuleDefinitionNode(className, body, isPublic);
    }

    private List<SyntaxNode> ParseModuleBody(SyntaxToken moduleName, bool isStatic) {
        List<SyntaxNode> nodes = new();

        while (Current is { Type: SyntaxType.Keyword, Text: "fld" })
        {
            Position++;

            var isPublic = false;

            if (Current is { Type: SyntaxType.Keyword, Text: "pub" })
            {
                Position++;
                isPublic = true;
            }

            var nameToken = MatchToken(SyntaxType.Identifier);
            var type = MatchToken(SyntaxType.Identifier);

            if (Current.Type == SyntaxType.Equals)
                throw new InvalidOperationException("Can not initialize a field directly!");

            nodes.Add(new ModuleFieldDefinitionNode(isPublic, isStatic, nameToken, type));
        }

        while (Current is { Type: SyntaxType.Keyword, Text: "fn" })
        {
            Position++;

            var isNative = false;

            if (Current is { Type: SyntaxType.Keyword, Text: "native" }) {
                Position++;
                isNative = true;
            }

            var isPublic = false;

            if(Current is { Type: SyntaxType.Keyword, Text: "pub" }) {
                Position++;
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

    public SyntaxNode ParseExpression(SyntaxToken? secondTypeToken)
    {
        if (Current is { Type: SyntaxType.Keyword, Text: "var" })
        {
            Position++;

            var ident = MatchToken(SyntaxType.Identifier);
            var type = MatchToken(SyntaxType.Identifier);

            SyntaxToken? secondType = null;

            if (Current.Type is SyntaxType.Identifier)
                secondType = MatchToken(SyntaxType.Identifier);
            else if (Current.Type is not SyntaxType.Equals)
                throw new InvalidOperationException("Variable initialization needs an expression!");

            Position++;
            var expr = ParseExpression(secondType);
            return new InitVariableNode(ident, type, secondType, expr);
        }
        if (Current.Type == SyntaxType.Identifier && Peek(1).Type == SyntaxType.Equals)
        {
            var ident = MatchToken(SyntaxType.Identifier);
            MatchToken(SyntaxType.Equals);
            var expr = ParseExpression(secondTypeToken);
            return new AssignExpressionNode(ident, expr);
        }
        return ParseCompExpression(secondTypeToken);
    }

    public SyntaxNode ParseCompExpression(SyntaxToken? secondTypeToken)
    {
        if(Current.Type == SyntaxType.Bang) {
            Position++;
            return new UnaryExpressionNode(Peek(-1), ParseCompExpression(secondTypeToken));
        }
        return BinaryOperation(() => ParseArithmeticExpression(secondTypeToken),
            new List<SyntaxType>
            {
                SyntaxType.EqualsEquals, SyntaxType.LessThan, SyntaxType.LessThanEqu, SyntaxType.GreaterThan, SyntaxType.GreaterThanEqu
            });
    }

    public SyntaxNode ParseArithmeticExpression(SyntaxToken? secondTypeToken) {
        return BinaryOperation(() => ParseTermExpression(secondTypeToken), new() { SyntaxType.Plus, SyntaxType.Minus });
    }

    public SyntaxNode ParseTermExpression(SyntaxToken? secondTypeToken) {
        return BinaryOperation(() => ParseFactorExpression(secondTypeToken), new() { SyntaxType.Mul, SyntaxType.Div, SyntaxType.Mod });
    }

    public SyntaxNode ParseFactorExpression(SyntaxToken? secondTypeToken) {
        if(Current.Type is SyntaxType.Plus or SyntaxType.Minus or SyntaxType.Bang) {
            var tok = Current;
            Position++;
            var factor = ParseFactorExpression(secondTypeToken);
            return new UnaryExpressionNode(tok, factor);
        }

        return ParsePowerExpression(secondTypeToken);
    }

    public SyntaxNode ParsePowerExpression(SyntaxToken? secondTypeToken) {
        return BinaryOperation(() => ParseDotExpression(secondTypeToken), new() { SyntaxType.Pow }, () => ParseFactorExpression(secondTypeToken));
    }

    public SyntaxNode ParseDotExpression(SyntaxToken? secondTypeToken) {
        var callNode = ParseCallExpression(secondTypeToken);
        DotNode accessStack = new(callNode);

        if (Current.Type is SyntaxType.Dot) {
            while (Current.Type is SyntaxType.Dot) {
                Position++;

                if (Current.Type is SyntaxType.Identifier) {
                    if (Peek(1).Type is SyntaxType.Equals) {
                        var ident = MatchToken(SyntaxType.Identifier);
                        MatchToken(SyntaxType.Equals);
                        var expr = ParseExpression(secondTypeToken);

                        accessStack.NextNodes.Add(new AssignExpressionNode(ident, expr));
                    } else {
                        var n = ParseCallExpression(secondTypeToken);
                        accessStack.NextNodes.Add(n);
                    }
                }
            }
        } else return callNode;

        return accessStack;
    }

    public SyntaxNode ParseCallExpression(SyntaxToken? secondTypeToken) {
        var atomNode = ParseCastExpression(secondTypeToken);

        if(Current.Type is SyntaxType.LParen) {
            Position++;

            List<SyntaxNode> argumentNodes = new();

            if(Current.Type is SyntaxType.RParen) {
                Position++;
            }else {
                argumentNodes.Add(ParseExpression(secondTypeToken));

                while(Current.Type is SyntaxType.Comma) {
                    Position++;

                    argumentNodes.Add(ParseExpression(secondTypeToken));
                }

                MatchToken(SyntaxType.RParen);
            }

            return new CallNode(atomNode, argumentNodes);
        }

        return atomNode;
    }

    public SyntaxNode ParseCastExpression(SyntaxToken? secondTypeToken)
    {
        if (Current.Type is SyntaxType.LParen)
        {
            MatchToken(SyntaxType.LParen);
            var ident = MatchToken(SyntaxType.Identifier);

            if (ident.Text is not "int" and not "float")
                throw new Exception($"Can not cast to: {ident.Text}");

            MatchToken(SyntaxType.RParen);

            var node = ParseCastExpression(secondTypeToken);
            return new CastNode(ident, node);
        }
        return ParseAtomExpression(secondTypeToken);
    }

    public SyntaxNode ParseAtomExpression(SyntaxToken? secondTypeToken)
    {
        switch (Current.Type)
        {
            case SyntaxType.Int:
                Position++;
                return new IntLiteralNode(Peek(-1));
            case SyntaxType.Float:
                Position++;
                return new FloatLiteralNode(Peek(-1));
            case SyntaxType.String:
                Position++;
                return new StringLiteralNode(Peek(-1));
            case SyntaxType.Identifier when Peek(1).Type is SyntaxType.LSqBracket:
                return ParseArrayIndexExpression(secondTypeToken);
            case SyntaxType.Identifier:
                Position++;
                return new IdentifierNode(Peek(-1));
            case SyntaxType.LParen:
            {
                Position++;
                var expr = ParseExpression(secondTypeToken);
                MatchToken(SyntaxType.RParen);
                return expr; // TODO: Do we have to create a ParenthisizedExpr? (probably not, but what if we do?)
            }
            case SyntaxType.LSqBracket:
                return ParseArrayExpression(secondTypeToken);
            case SyntaxType.Keyword when Current.Text == "if":
                return ParseIfExpression(secondTypeToken);
        }

        if (Current.Type is SyntaxType.Keyword && Current.Text == "for")
            return ParseForExpression(secondTypeToken);
        if (Current.Type is SyntaxType.Keyword && Current.Text == "while")
            return ParseWhileExpression(secondTypeToken);
        if (Current.Type is SyntaxType.Keyword && Current.Text == "fld")
            return ParseFieldExpression();
        if (Current.Type is SyntaxType.Keyword && Current.Text == "new")
            return ParseInstantiateExpression(secondTypeToken);

        throw new Exception($"Unexpected token {Current.Type} at pos {Current.Position} in atom expression!");
    }

    public SyntaxNode ParseArrayIndexExpression(SyntaxToken? secondTypeToken)
    {
        var ident = MatchToken(SyntaxType.Identifier);
        MatchToken(SyntaxType.LSqBracket);
        var expr = ParseExpression(secondTypeToken);
        MatchToken(SyntaxType.RSqBracket);

        SyntaxNode? setNode = null;

        if (Current.Type is SyntaxType.Equals)
        {
            Position++;
            setNode = ParseExpression(secondTypeToken);
        }

        return new ArrayIndexNode(new IdentifierNode(ident), expr, setNode);
    }

    public SyntaxNode ParseArrayExpression(SyntaxToken? secondTypeToken)
    {
        if (secondTypeToken is null)
            throw new InvalidOperationException("Invalid array type!");

        MatchToken(SyntaxType.LSqBracket);

        if (Current.Type == SyntaxType.RSqBracket)
            throw new InvalidOperationException("Array length not specified!");

        var expr = ParseExpression(secondTypeToken);
        MatchToken(SyntaxType.RSqBracket);

        return new ArrayNode(secondTypeToken, expr);
    }

    public SyntaxNode ParseIfExpression(SyntaxToken? secondTypeToken) {
        MatchKeyword("if");

        IfNode node = new();

        MatchToken(SyntaxType.LParen);
        var initialCond = ParseExpression(secondTypeToken);
        MatchToken(SyntaxType.RParen);

        var initialBlock = ParseScopedStatements();

        node.AddCase(initialCond, initialBlock);

        while (Current is { Type: SyntaxType.Keyword, Text: "elif" }) {
            Position++;

            MatchToken(SyntaxType.LParen);
            var cond = ParseExpression(secondTypeToken);
            MatchToken(SyntaxType.RParen);
            var block = ParseScopedStatements();

            node.AddCase(cond, block);
        }

        if(Current is { Type: SyntaxType.Keyword, Text: "else" }) {
            Position++;
            var block = ParseScopedStatements();

            node.AddCase(new BoolNode(true), block);
        }

        return node;
    }

    public SyntaxNode ParseForExpression(SyntaxToken? secondTypeToken) {
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

    public SyntaxNode ParseWhileExpression(SyntaxToken? secondTypeToken) {
        MatchKeyword("while");

        MatchToken(SyntaxType.LParen);
        var condNode = ParseExpression(secondTypeToken);
        MatchToken(SyntaxType.RParen);
        var block = ParseScopedStatements();

        return new WhileNode(condNode, block);
    }

    public SyntaxNode ParseFunctionDefinition() {
        MatchKeyword("fn");

        var isNative = false;

        if (Current is { Type: SyntaxType.Keyword, Text: "native" }) {
            Position++;
            isNative = true;
        }

        var isPublic = false;

        if(Current is { Type: SyntaxType.Keyword, Text: "pub" }) {
            Position++;
            isPublic = true;
        }

        var nameToken = MatchToken(SyntaxType.Identifier);
        var args = ParseFunctionArgs();
        var returnType = MatchToken(SyntaxType.Identifier);
        var block = ParseScopedStatements();

        return new FunctionDefinitionNode(nameToken, args, returnType, block, isPublic, isNative);
    }

    public SyntaxNode ParseFieldExpression()
    {
        MatchKeyword("fld");

        var isPublic = false;

        if (Current is { Type: SyntaxType.Keyword, Text: "pub" })
        {
            Position++;
            isPublic = true;
        }

        var nameToken = MatchToken(SyntaxType.Identifier);
        var type = MatchToken(SyntaxType.Identifier);

        if (Current.Type == SyntaxType.Equals)
            throw new InvalidOperationException("Can not initialize a field directly!");

        return new FieldDefinitionNode(isPublic, nameToken, type);
    }

    public SyntaxNode ParseInstantiateExpression(SyntaxToken? secondTypeToken) {
        Position++;
        var ident = MatchToken(SyntaxType.Identifier);

        List<SyntaxNode> argumentNodes = new();

        if (Current.Type is SyntaxType.LParen) {
            Position++;

            if (Current.Type is SyntaxType.RParen) {
                Position++;
            } else {
                argumentNodes.Add(ParseExpression(secondTypeToken));

                while (Current.Type is SyntaxType.Comma) {
                    Position++;

                    argumentNodes.Add(ParseExpression(secondTypeToken));
                }

                MatchToken(SyntaxType.RParen);
            }
        }

        return new InstantiateNode(ident, argumentNodes);
    }

    public Dictionary<SyntaxToken, SyntaxToken> ParseFunctionArgs() {
        MatchToken(SyntaxType.LParen);

        var args = new Dictionary<SyntaxToken, SyntaxToken>();

        if (Current.Type != SyntaxType.RParen)
        {
            var type = MatchToken(SyntaxType.Identifier);
            var ident = MatchToken(SyntaxType.Identifier);
            args.Add(type, ident);

            while (Current.Type == SyntaxType.Comma)
            {
                Position++;
                type = MatchToken(SyntaxType.Identifier);
                ident = MatchToken(SyntaxType.Identifier);
                args.Add(type, ident);
            }
        }

        MatchToken(SyntaxType.RParen);
        return args;
    }
    public SyntaxNode BinaryOperation(Func<SyntaxNode> leftParse, List<SyntaxType> allowedTypes, Func<SyntaxNode> rightParse = null) {
        var left = leftParse();
        while (allowedTypes.Contains(Current.Type)) {
            var operatorToken = Current;
            Position++;
            var right = (rightParse ?? leftParse)();

            left = new BinaryExpressionNode(left, operatorToken, right);
        }

        return left;
    }
}
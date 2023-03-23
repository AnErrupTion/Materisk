using Materisk.BuiltinTypes;
using Materisk.Parsing.Nodes;

namespace Materisk.Parsing;

public class Parser {
    public List<SyntaxToken> Tokens { get; }
    public int Position;

    public SyntaxToken Current => Peek();

    public SyntaxToken Peek(int off = 0) {
        if (Position + off >= Tokens.Count || Position + off < 0) return new(SyntaxType.BadToken, 0, null, string.Empty);
        return Tokens[Position + off];
    }

    public SyntaxToken MatchToken(SyntaxType type) {
        if(Current.Type == type) {
            Position++;
            return Peek(-1);
        }

        throw new Exception("Unexpected token " + Current.Type + "; expected " + type);
    }

    public SyntaxToken MatchTokenWithValue(SyntaxType type, object value)
    {
        if (Current.Type == type && Current.Value == value) {
            Position++;
            return Peek(-1);
        }

        throw new Exception("Unexpected token " + Current.Type + "; expected " + type + " with value " + value);
    }

    public SyntaxToken MatchKeyword(string value) {
        if (Current.Type == SyntaxType.Keyword && Current.Text == value) {
            Position++;
            return Peek(-1);
        }

        throw new Exception("Unexpected token " + Current.Type + "; expected Keyword with value " + value);
    }

    public Parser(List<SyntaxToken> tokens) {
        Tokens = tokens;
    }

    public SyntaxNode Parse() {
        return ParseStatements();
    }

    public SyntaxNode ParseStatements() {
        List<SyntaxNode> nodes = new();

        while(Current.Type != SyntaxType.EOF) {
            nodes.Add(ParseStatement());
        }

        return new BlockNode(nodes, false);
    }

    public SyntaxNode ParseScopedStatements() {
        MatchToken(SyntaxType.LBraces);
        List<SyntaxNode> nodes = new();

        while(Current.Type != SyntaxType.RBraces) {
            if (Current.Type == SyntaxType.EOF) throw new Exception("Unclosed block at " + Current.Position);

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
                var ret = new ReturnNode(ParseExpression());
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
        if (Current is { Type: SyntaxType.Keyword, Text: "mod" }) {
            return ParseModuleDefinition();
        }
        var exprNode = ParseExpression();
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
        var body = ParseClassBody(className, isStatic);
        MatchToken(SyntaxType.RBraces);

        return new ClassDefinitionNode(className, body, isPublic);
    }

    private List<SyntaxNode> ParseClassBody(SyntaxToken className, bool isStatic) {
        List<SyntaxNode> nodes = new();

        while(Current is { Type: SyntaxType.Keyword, Text: "fn" }) {
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
            var body = ParseScopedStatements();

            nodes.Add(new ClassFunctionDefinitionNode(className, name, args, body, isStatic, isPublic, isNative));
        }

        return nodes;
    }

    public SyntaxNode ParseExpression()
    {
        if(Current is { Type: SyntaxType.Keyword, Text: "var" }) {
            var fixedType = true;
            Position++;

            if(Current.Type == SyntaxType.Mod) {
                fixedType = false;
                Position++;
            }

            var ident = MatchToken(SyntaxType.Identifier);

            if(Current.Type == SyntaxType.Equals) {
                Position++;
                var expr = ParseExpression();
                return new InitVariableNode(ident, expr, fixedType);
            }
            return new InitVariableNode(ident, fixedType);
        }
        if(Current.Type == SyntaxType.Identifier && Peek(1).Type == SyntaxType.Equals) {
            var ident = MatchToken(SyntaxType.Identifier);
            MatchToken(SyntaxType.Equals);
            var expr = ParseExpression();
            return new AssignVariableNode(ident, expr);
        }
        return ParseCompExpression();
    }

    public SyntaxNode ParseCompExpression()
    {
        if(Current.Type == SyntaxType.Bang) {
            Position++;
            return new UnaryExpressionNode(Peek(-1), ParseCompExpression());
        }
        return BinaryOperation(ParseArithmeticExpression,
            new List<SyntaxType>
            {
                SyntaxType.EqualsEquals, SyntaxType.LessThan, SyntaxType.LessThanEqu, SyntaxType.GreaterThan, SyntaxType.GreaterThanEqu
            });
    }

    public SyntaxNode ParseArithmeticExpression() {
        return BinaryOperation(ParseTermExpression, new() { SyntaxType.Plus, SyntaxType.Minus });
    }

    public SyntaxNode ParseTermExpression() {
        return BinaryOperation(ParseFactorExpression, new() { SyntaxType.Mul, SyntaxType.Div, SyntaxType.Mod, SyntaxType.Idx });
    }

    public SyntaxNode ParseFactorExpression() {
        if(Current.Type is SyntaxType.Plus or SyntaxType.Minus or SyntaxType.Bang) {
            var tok = Current;
            Position++;
            var factor = ParseFactorExpression();
            return new UnaryExpressionNode(tok, factor);
        }

        return ParsePowerExpression();
    }

    public SyntaxNode ParsePowerExpression() {
        return BinaryOperation(ParseDotExpression, new() { SyntaxType.Pow }, () => { return ParseFactorExpression(); });
    }

    public SyntaxNode ParseDotExpression() {
        var callNode = ParseCallExpression();
        DotNode accessStack = new(callNode);

        if (Current.Type is SyntaxType.Dot) {
            while (Current.Type is SyntaxType.Dot) {
                Position++;

                if (Current.Type is SyntaxType.Identifier) {
                    if (Peek(1).Type is SyntaxType.Equals) {
                        var ident = MatchToken(SyntaxType.Identifier);
                        MatchToken(SyntaxType.Equals);
                        var expr = ParseExpression();

                        accessStack.NextNodes.Add(new AssignVariableNode(ident, expr));
                    } else {
                        var n = ParseCallExpression();
                        accessStack.NextNodes.Add(n);
                    }
                }
            }
        } else return callNode;

        return accessStack;
    }

    public SyntaxNode ParseCallExpression() {
        var atomNode = ParseCastExpression();

        if(Current.Type is SyntaxType.LParen) {
            Position++;

            List<SyntaxNode> argumentNodes = new();

            if(Current.Type is SyntaxType.RParen) {
                Position++;
            }else {
                argumentNodes.Add(ParseExpression());

                while(Current.Type is SyntaxType.Comma) {
                    Position++;

                    argumentNodes.Add(ParseExpression());
                }

                MatchToken(SyntaxType.RParen);
            }

            return new CallNode(atomNode, argumentNodes);
        }

        return atomNode;
    }

    public SyntaxNode ParseCastExpression()
    {
        if(Current.Type is SyntaxType.LessThan) {
            MatchToken(SyntaxType.LessThan);
            var ident = MatchToken(SyntaxType.Identifier);

            if (ident.Text is not "int" and not "float" and not "list" and not "string") throw new Exception("Can not cast to " + ident.Text);

            MatchToken(SyntaxType.GreaterThan);

            var node = ParseCastExpression();
            return new CastNode(ident, node);
        }
        return ParseAtomExpression();
    }

    public SyntaxNode ParseAtomExpression()
    {
        if (Current.Type is SyntaxType.Int) {
            Position++;
            return new IntLiteralNode(Peek(-1));
        }

        if (Current.Type is SyntaxType.Float) {
            Position++;
            return new FloatLiteralNode(Peek(-1));
        }
        if (Current.Type is SyntaxType.String) {
            Position++;
            return new StringLiteralNode(Peek(-1));
        }
        if (Current.Type is SyntaxType.Identifier) {
            Position++;
            return new IdentifierNode(Peek(-1));
        }
        if (Current.Type is SyntaxType.LParen) {
            Position++;
            var expr = ParseExpression();

            MatchToken(SyntaxType.RParen);

            return expr; // TODO: Do we have to create a ParenthisizedExpr? (probably not, but what if we do?)
        }
        if (Current.Type is SyntaxType.LSqBracket) {
            return ParseListExpression();
        }
        if (Current.Type is SyntaxType.Keyword && Current.Text == "if") {
            return ParseIfExpression();
        }
        if (Current.Type is SyntaxType.Keyword && Current.Text == "for") {
            return ParseForExpression();
        }
        if (Current.Type is SyntaxType.Keyword && Current.Text == "while") {
            return ParseWhileExpression();
        }
        if (Current.Type is SyntaxType.Keyword && Current.Text == "fn") {
            return ParseFunctionExpression();
        }
        if (Current.Type is SyntaxType.Keyword && Current.Text == "new") {
            return ParseInstantiateExpression();
        }
        throw new Exception($"Unexpected token {Current.Type} at pos {Current.Position} in atom expression!");
    }

    public SyntaxNode ParseListExpression() {
        MatchToken(SyntaxType.LSqBracket);

        List<SyntaxNode> list = new();

        if (Current.Type == SyntaxType.RSqBracket) {
            MatchToken(SyntaxType.RSqBracket);
        } else {
            var expr = ParseExpression();
            list.Add(expr);

            while (Current.Type == SyntaxType.Comma) {
                Position++;
                expr = ParseExpression();
                list.Add(expr);
            }

            MatchToken(SyntaxType.RSqBracket);
        }

        return new ListNode(list);
    }

    public SyntaxNode ParseIfExpression() {
        MatchKeyword("if");

        IfNode node = new();

        MatchToken(SyntaxType.LParen);
        var initialCond = ParseExpression();
        MatchToken(SyntaxType.RParen);

        var initialBlock = ParseScopedStatements();

        node.AddCase(initialCond, initialBlock);

        while (Current.Type == SyntaxType.Keyword && (string)Current.Value == "elseif") {
            Position++;

            MatchToken(SyntaxType.LParen);
            var cond = ParseExpression();
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

    public SyntaxNode ParseForExpression() {
        MatchKeyword("for");

        MatchToken(SyntaxType.LParen);
        var initialExpressionNode = ParseExpression();
        MatchToken(SyntaxType.Semicolon);
        var condNode = ParseExpression();
        MatchToken(SyntaxType.Semicolon);
        var stepNode = ParseExpression();
        MatchToken(SyntaxType.RParen);
        var block = ParseScopedStatements();

        return new ForNode(initialExpressionNode, condNode, stepNode, block);
    }

    public SyntaxNode ParseWhileExpression() {
        MatchKeyword("while");

        MatchToken(SyntaxType.LParen);
        var condNode = ParseExpression();
        MatchToken(SyntaxType.RParen);
        var block = ParseScopedStatements();

        return new WhileNode(condNode, block);
    }

    public SyntaxNode ParseFunctionExpression() {
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

        SyntaxToken? nameToken = null;
        if(Current.Type == SyntaxType.Identifier)
            nameToken = MatchToken(SyntaxType.Identifier);

        var args = ParseFunctionArgs();

        var block = ParseScopedStatements();

        return new FunctionDefinitionNode(nameToken, args, block, isPublic, isNative);
    }

    public SyntaxNode ParseInstantiateExpression() {
        Position++;
        var ident = MatchToken(SyntaxType.Identifier);

        List<SyntaxNode> argumentNodes = new();

        if (Current.Type is SyntaxType.LParen) {
            Position++;

            if (Current.Type is SyntaxType.RParen) {
                Position++;
            } else {
                argumentNodes.Add(ParseExpression());

                while (Current.Type is SyntaxType.Comma) {
                    Position++;

                    argumentNodes.Add(ParseExpression());
                }

                MatchToken(SyntaxType.RParen);
            }
        }

        return new InstantiateNode(ident, argumentNodes);
    }

    public List<SyntaxToken> ParseFunctionArgs() {
        MatchToken(SyntaxType.LParen);

        List<SyntaxToken> args = new();

        if (Current.Type == SyntaxType.RParen) {
            MatchToken(SyntaxType.RParen);
        } else {
            var ident = MatchToken(SyntaxType.Identifier);
            args.Add(ident);

            while (Current.Type == SyntaxType.Comma) {
                Position++;
                ident = MatchToken(SyntaxType.Identifier);
                args.Add(ident);
            }

            MatchToken(SyntaxType.RParen);
        }

        return args;
    }
    public SyntaxNode BinaryOperation(Func<SyntaxNode> leftParse, List<SyntaxType> allowedTypes, Func<SyntaxNode> rightParse = null) {
        var left = leftParse();
        SyntaxNode right;
        while (allowedTypes.Contains(Current.Type)) {
            var operatorToken = Current;
            Position++;
            right = (rightParse ?? leftParse)();

            left = new BinaryExpressionNode(left, operatorToken, right);
        }

        return left;
    }
}

internal class ImportNode : SyntaxNode {
    private readonly SyntaxToken path;

    public ImportNode(SyntaxToken path) {
        this.path = path;
    }

    public override NodeType Type => NodeType.Import;

    public override SValue Evaluate(Scope scope) {
        if (!File.Exists(path.Text)) throw new Exception($"Failed to import '{path.Text}': File not found");
        var text = File.ReadAllText(path.Text);

        Interpreter ip = new();
        InterpreterResult res = new();

        try {
            ip.Interpret(text, ref res);

            // copy export table

            foreach(var kvp in ip.GlobalScope.ExportTable) {
                if (scope.Get(kvp.Key) != null) throw new Exception($"Failed to import '{path.Text}': Import conflict; file exports '{kvp.Key}' but that identifier is already present in the current scope.");

                scope.Set(kvp.Key, kvp.Value);
            }
        }catch(Exception ex) {
            throw new Exception($"Failed to import '{path.Text}': {ex.Message}");
        }

        return res.LastValue;
    }

    public override IEnumerable<SyntaxNode> GetChildren() {
        throw new NotImplementedException();
    }
}
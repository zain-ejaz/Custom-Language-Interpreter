using System;
using System.Globalization;

namespace LanguageDesign
{
    public class Parser
    {
        // The lexer used for tokenizing the input
        private readonly Lexer _lexer;

        public Parser(Lexer lexer) => _lexer = lexer;

        public AstNode ParseExpression()
        {
            // Parses an expression and returns the resulting abstract syntax tree (AST)
            var left = ParseLogicalTerm();

            while (_lexer.CurrentToken.Type == TokenType.Or)
            {
                // Continues parsing logical terms while encountering logical OR operators
                var token = _lexer.CurrentToken;
                _lexer.NextToken();
                var right = ParseLogicalTerm();
                left = new LogicalOperationNode
                {
                    Left = left,
                    Right = right,
                    Operator = token.Type
                };
            }

            return left;
        }

        private AstNode ParseLogicalTerm()
        {
            // Parses a logical term and returns the resulting AST
            var left = ParseLogicalFactor();

            while (_lexer.CurrentToken.Type == TokenType.And)
            {
                // Continues parsing logical factors while encountering logical AND operators
                var token = _lexer.CurrentToken;
                _lexer.NextToken();
                var right = ParseLogicalFactor();
                left = new LogicalOperationNode
                {
                    Left = left,
                    Right = right,
                    Operator = token.Type
                };
            }

            return left;
        }

        private AstNode ParseLogicalFactor()
        {
            // Parses a logical factor and returns the resulting AST
            if (_lexer.CurrentToken.Type == TokenType.Not)
            {
                // Handles logical NOT operators
                var token = _lexer.CurrentToken;
                _lexer.NextToken();
                var operand = ParseComparisonExpression();
                return new LogicalOperationNode
                {
                    Left = operand,
                    Right = null,
                    Operator = token.Type
                };
            }

            return ParseComparisonExpression();
        }

        private AstNode ParseComparisonExpression()
        {
            // Parses a comparison expression and returns the resulting AST
            var left = ParseRelationExpression();

            while (_lexer.CurrentToken.Type is TokenType.Equals
                or TokenType.NotEquals)
            {
                // Continues parsing relation expressions while encountering equality or inequality operators
                var token = _lexer.CurrentToken;
                _lexer.NextToken();
                var right = ParseExpression();
                left = new ComparisonOperationNode
                {
                    Left = left,
                    Right = right,
                    Operator = token.Type
                };
            }

            return left;
        }

        private AstNode ParseRelationExpression()
        {
            // Parses a relation expression and returns the resulting AST
            var left = ParseArithmeticExpression();

            while (_lexer.CurrentToken.Type is TokenType.LessThan or TokenType.GreaterThan)
            {
                // Continues parsing arithmetic expressions while encountering less-than or greater-than operators
                var token = _lexer.CurrentToken;
                _lexer.NextToken();
                var right = ParseArithmeticExpression();
                left = new ComparisonOperationNode
                {
                    Left = left,
                    Right = right,
                    Operator = token.Type
                };
            }

            return left;
        }

        public AstNode ParseArithmeticExpression()
        {
            // Parses an arithmetic expression and returns the resulting AST
            var left = ParseTerm();

            while (_lexer.CurrentToken.Type is TokenType.Add or TokenType.Subtract)
            {
                // Continues parsing terms while encountering addition or subtraction operators
                var token = _lexer.CurrentToken;
                _lexer.NextToken();
                var right = ParseTerm();
                left = new BinaryOperationNode
                {
                    Left = left,
                    Right = right,
                    Operator = token.Type
                };
            }

            return left;
        }

        public AstNode ParseStatement()
        {
            // Parses a statement and returns the resulting AST node
            AstNode node;

            switch (_lexer.CurrentToken.Type)
            {
                case TokenType.Identifier:
                    // Handles an assignment statement
                    var variableName = _lexer.CurrentToken.StringValue;
                    _lexer.NextToken();
                    if (_lexer.CurrentToken.Type != TokenType.Assignment)
                        throw new Exception("Expected '=' in assignment statement.");
                    _lexer.NextToken();
                    var expression = ParseExpression();
                    node = new AssignmentNode { VariableName = variableName, Expression = expression };
                    break;
                case TokenType.Print:
                    // Handles a print statement
                    _lexer.NextToken();
                    // Parse the expression inside the print statement
                    var printExpression = ParseExpression();
                    node = new PrintNode { Expression = printExpression };
                    break;
                default:
                    throw new Exception("Expected assignment or print statement.");
            }

            if (_lexer.CurrentToken.Type != TokenType.Semicolon)
                throw new Exception("Expected ';' after statement.");
            _lexer.NextToken();

            return node;
        }

        private AstNode ParseTerm()
        {
            // Parses a term and returns the resulting AST
            var left = ParseFactor();

            while (_lexer.CurrentToken.Type is TokenType.Multiply or TokenType.Divide)
            {
                // Continues parsing factors while encountering multiplication or division operators
                var token = _lexer.CurrentToken;
                _lexer.NextToken();
                var right = ParseFactor();
                left = new BinaryOperationNode
                {
                    Left = left,
                    Right = right,
                    Operator = token.Type
                };
            }
            return left;
        }

        private AstNode ParseFactor()
        {
            // Parses a factor and returns the resulting AST
            if (_lexer.CurrentToken.Type is TokenType.Add or TokenType.Subtract)
            {
                // Handles unary plus and minus operators
                var token = _lexer.CurrentToken;
                _lexer.NextToken();
                return new UnaryOperationNode
                {
                    Operand = ParseFactor(),
                    Operator = token.Value[0]
                };
            }

            return ParsePrimary();
        }

        private AstNode ParsePrimary()
        {
            // Parses a primary expression and returns the resulting AST node
            switch (_lexer.CurrentToken.Type)
            {
                case TokenType.Number:
                    {
                        // Handles a numeric literal
                        var token = _lexer.CurrentToken;
                        _lexer.NextToken();
                        return new NumberNode
                        {
                            Number = double.Parse(token.Value, CultureInfo.InvariantCulture)
                        };
                    }
                case TokenType.True:
                case TokenType.False:
                    {
                        // Handles a boolean literal (true or false)
                        var token = _lexer.CurrentToken;
                        _lexer.NextToken();
                        return new BooleanNode
                        {
                            Value = token.BooleanValue.Value
                        };
                    }
                case TokenType.String:
                    {
                        // Handles a string literal
                        var token = _lexer.CurrentToken;
                        _lexer.NextToken();
                        return new StringNode
                        {
                            Value = token.StringValue
                        };
                    }
                case TokenType.LeftParen:
                    {
                        // Handles an expression enclosed in parentheses
                        _lexer.NextToken();
                        var innerNode = ParseExpression();
                        if (_lexer.CurrentToken.Type != TokenType.RightParen)
                        {
                            throw new Exception("Mismatched parentheses");
                        }
                        // Move to the next token after the right parenthesis
                        _lexer.NextToken();
                        return innerNode;
                    }
                case TokenType.Identifier:
                    // Handles a variable reference
                    var variableName = _lexer.CurrentToken.StringValue;
                    var node = new VariableNode { Name = variableName };
                    _lexer.NextToken();
                    return node;
                default:
                    throw new Exception("Syntax error");
            }
        }
    }
}

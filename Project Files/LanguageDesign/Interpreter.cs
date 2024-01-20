using System;
using System.Globalization;

namespace LanguageDesign
{
    // Base class for all nodes in the Abstract Syntax Tree (AST)
    public abstract class AstNode
    {
        // Evaluates the current node using a given symbol table
        public abstract object Evaluate(SymbolTable symbolTable);
    }

    // Dictionary that stores variable names and their values
    public class SymbolTable
    {
        private Dictionary<string, object> _symbols = new Dictionary<string, object>();

        // Method to retrieve the value of a symbol from the symbol table
        public object Get(string name)
        {
            if (_symbols.TryGetValue(name, out var value))
            {
                return value;
            }

            throw new Exception($"Variable '{name}' is not defined");
        }

        // Method to set the value of a symbol in the symbol table
        public void Set(string name, object value)
        {
            _symbols[name] = value;
        }
    }

    // Represents a number
    public class NumberNode : AstNode
    {
        public double Number { get; set; }

        // Returns the number
        public override object Evaluate(SymbolTable symbolTable) => Number;
    }

    // Represents a boolean value
    public class BooleanNode : AstNode
    {
        public bool Value { get; set; }

        // Returns the boolean value
        public override object Evaluate(SymbolTable symbolTable) => Value;
    }

    // Represents a string
    public class StringNode : AstNode
    {
        public string Value { get; set; }

        // Returns the string
        public override object Evaluate(SymbolTable symbolTable) => Value;
    }

    // Represents a variable
    public class VariableNode : AstNode
    {
        public string Name { get; set; }

        // Returns the value of the variable
        public override object Evaluate(SymbolTable symbolTable)
        {
            return symbolTable.Get(Name);
        }
    }

    // Represents an assignment operation
    public class AssignmentNode : AstNode
    {
        public string VariableName { get; set; }
        public AstNode Expression { get; set; }

        // Evaluates the expression and assigns the result to the variable
        public override object Evaluate(SymbolTable symbolTable)
        {
            var value = Expression.Evaluate(symbolTable);
            symbolTable.Set(VariableName, value);
            return value;
        }
    }

    // Represents a print operation
    public class PrintNode : AstNode
    {
        public AstNode Expression { get; set; }

        // Evaluates the expression, prints the result, and returns null
        public override object Evaluate(SymbolTable symbolTable)
        {
            var value = Expression.Evaluate(symbolTable);
            Console.WriteLine(value);
            return null;
        }
    }

    // Represents a unary operation
    public class UnaryOperationNode : AstNode
    {
        public char Operator { get; set; }
        public AstNode Operand { get; set; }

        // Evaluates the operand and applies the unary operator
        public override object Evaluate(SymbolTable symbolTable)
        {
            var number = (double)Operand.Evaluate(symbolTable);
            return Operator == '+' ? +number : -number;
        }
    }

    // Represents a binary operation
    public class BinaryOperationNode : AstNode
    {
        public TokenType Operator { get; set; }
        public AstNode Left { get; set; }
        public AstNode Right { get; set; }

        // Evaluates the operands and applies the binary operator
        public override object Evaluate(SymbolTable symbolTable)
        {
            // If both operands are strings and operator is add, concatenate the strings
            if (Operator == TokenType.Add && Left is StringNode && Right is StringNode)
            {
                var left = (Left as StringNode).Value;
                var right = (Right as StringNode).Value;
                return left + right;
            }

            // If either operand is a string and the operator is add, concatenate the string representation of the operands
            if (Operator == TokenType.Add && (Left is StringNode || Right is StringNode))
            {
                var left = Left.Evaluate(symbolTable).ToString();
                var right = Right.Evaluate(symbolTable).ToString();
                return left + right;
            }

            // If either operand is a string and the operator is not add, throw an exception
            if ((Left is StringNode || Right is StringNode) && Operator != TokenType.Add)
            {
                throw new InvalidOperationException("Can't use non-addition operators with strings");
            }

            // Otherwise, treat the operands as numbers and perform the appropriate number operation
            var leftNum = Convert.ToDouble(Left.Evaluate(symbolTable));
            var rightNum = Convert.ToDouble(Right.Evaluate(symbolTable));

            return Operator switch
            {
                TokenType.Add => leftNum + rightNum,
                TokenType.Subtract => leftNum - rightNum,
                TokenType.Multiply => leftNum * rightNum,
                TokenType.Divide => leftNum / rightNum,
                _ => throw new Exception($"Unexpected operator: {Operator}")
            };
        }
    }

    // Represents a logical operation
    public class LogicalOperationNode : AstNode
    {
        public AstNode Left { get; set; }
        public AstNode Right { get; set; }
        public TokenType Operator { get; set; }

        // Evaluates the operands and applies the logical operator
        public override object Evaluate(SymbolTable symbolTable)
        {
            // If operator is NOT, perform NOT operation on Left node's evaluated result
            if (Operator == TokenType.Not)
            {
                return !Convert.ToBoolean(Left.Evaluate(symbolTable));
            }

            // Evaluate Left and Right nodes.
            var left = Convert.ToBoolean(Left.Evaluate(symbolTable));
            var right = Convert.ToBoolean(Right.Evaluate(symbolTable));

            // Perform AND or OR operation depending on Operator
            return Operator switch
            {
                TokenType.And => left && right,
                TokenType.Or => left || right,
                _ => throw new Exception($"Unknown operator: {Operator}")
            };
        }
    }

    // Represents a comparison operation
    public class ComparisonOperationNode : AstNode
    {
        public AstNode Left { get; set; }
        public AstNode Right { get; set; }
        public TokenType Operator { get; set; }

        // Evaluates the operands and applies the comparison operator
        public override object Evaluate(SymbolTable symbolTable)
        {
            // Evaluate Left and Right nodes.
            object leftValue = Left.Evaluate(symbolTable);
            object rightValue = Right.Evaluate(symbolTable);

            // If both are strings, perform comparison based on Operator
            if (leftValue is string leftString && rightValue is string rightString)
            {
                switch (Operator)
                {
                    case TokenType.Equals:
                        return leftString == rightString;
                    case TokenType.NotEquals:
                        return leftString != rightString;
                    default:
                        throw new Exception($"Unexpected operator: {Operator} for strings");
                }
            }
            // If one is a string, throw exception
            else if (leftValue is string || rightValue is string)
            {
                throw new Exception("Can't perform comparison between a string and a non-string");
            }
            // Otherwise, treat as numbers and perform comparison based on Operator
            else
            {
                var leftNum = Convert.ToDouble(leftValue);
                var rightNum = Convert.ToDouble(rightValue);

                return Operator switch
                {
                    TokenType.Equals => leftNum == rightNum,
                    TokenType.NotEquals => leftNum != rightNum,
                    TokenType.GreaterThan => leftNum > rightNum,
                    TokenType.LessThan => leftNum < rightNum,
                    _ => throw new Exception($"Unknown operator: {Operator}")
                };
            }
        }
    }
}

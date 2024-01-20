using System;
using System.Globalization;

namespace LanguageDesign
{
    public enum TokenType
    {
        // Token types
        EOF,
        Add,
        Subtract,
        Multiply,
        Divide,
        Number,
        LeftParen,
        RightParen,
        True,
        False,
        Equals,
        NotEquals,
        LessThan,
        GreaterThan,
        And,
        Or,
        Not,
        String,
        Identifier,
        Assignment,
        Semicolon,
        Print,
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public bool? BooleanValue { get; set; }
        public string StringValue { get; set; }
    }

    public class Lexer
    {
        // Input string to be tokenized
        private readonly string _input;
        private int _position;

        // Mapping of operators to their respective token types
        private static readonly Dictionary<char, TokenType> OperatorMap = new Dictionary<char, TokenType>()
    {
        {'!', TokenType.Not },
        {'<', TokenType.LessThan },
        {'>', TokenType.GreaterThan },
        {'=', TokenType.Equals },
        {'&', TokenType.And },
        {'|', TokenType.Or },
    };

        public Token CurrentToken { get; private set; }

        // Initialize Lexer with input string
        public Lexer(string input)
        {
            _input = input;
            _position = 0;
            NextToken();
        }

        // Method to advance to the next token in the input string
        public void NextToken()
        {
            SkipWhiteSpace();

            // If we have consumed all characters in the input, set the current token to EOF (End of File)
            if (_position >= _input.Length)
            {
                CurrentToken = new Token { Type = TokenType.EOF };
                return;
            }

            var current = _input[_position];

            if (char.IsDigit(current))
            {
                var number = ParseNumber();
                CurrentToken = new Token { Type = TokenType.Number, Value = number };
                return;
            }

            if (char.IsLetter(current))
            {
                ParseWord();
                return;
            }

            if (OperatorMap.ContainsKey(current))
            {
                ParseOperator();
                return;
            }

            if (current == '=')
            {
                CurrentToken = new Token { Type = TokenType.Assignment };
                _position++;
                return;
            }

            if (_input[_position] == '"')
            {
                ParseString();
                return;
            }

            if (current == ';')
            {
                CurrentToken = new Token { Type = TokenType.Semicolon };
                _position++;
                return;
            }

            // Switch cases for multiple operators 
            switch (current)
            {
                case '+':
                    CurrentToken = new Token { Type = TokenType.Add };
                    break;
                case '-':
                    if (_position + 1 < _input.Length && char.IsDigit(_input[_position + 1]))
                    {
                        var number = ParseNumber();
                        CurrentToken = new Token { Type = TokenType.Number, Value = number };
                    }
                    else
                    {
                        CurrentToken = new Token { Type = TokenType.Subtract, Value = current.ToString() };
                    }
                    break;
                case '*':
                    CurrentToken = new Token { Type = TokenType.Multiply, Value = current.ToString() };
                    break;
                case '/':
                    CurrentToken = new Token { Type = TokenType.Divide, Value = current.ToString() };
                    break;
                case '(':
                    CurrentToken = new Token { Type = TokenType.LeftParen, Value = current.ToString() };
                    break;
                case ')':
                    CurrentToken = new Token { Type = TokenType.RightParen, Value = current.ToString() };
                    break;
                default:
                    throw new Exception($"Unexpected character: {_input[_position]}");
            }

            _position++;
        }

        // Method to parse a number in the input string
        private string ParseNumber()
        {
            var start = _position;

            if (_input[_position] == '-' || _input[_position] == '+')
            {
                _position++;
            }

            while (_position < _input.Length && (char.IsDigit(_input[_position]) || _input[_position] == '.'))
            {
                _position++;
            }

            return _input.Substring(start, _position - start);
        }

        // Method to parse an operator in the input string
        private void ParseOperator()
        {
            var current = _input[_position];

            if (current == '!')
            {
                if (_position + 1 < _input.Length && _input[_position + 1] == '=')
                {
                    CurrentToken = new Token { Type = TokenType.NotEquals };
                    _position += 2;
                    return;
                }
                else
                {
                    CurrentToken = new Token { Type = TokenType.Not };
                    _position++;
                    return;
                }
            }
            else if (current == '=')
            {
                if (_position + 1 < _input.Length && _input[_position + 1] == '=')
                {
                    CurrentToken = new Token { Type = TokenType.Equals };
                    _position += 2;
                    return;
                }
                else
                {
                    // Treat single '=' as an assignment operator
                    CurrentToken = new Token { Type = TokenType.Assignment };
                    _position++;
                    return;
                }
            }

            if (OperatorMap.ContainsKey(current))
            {
                CurrentToken = new Token { Type = OperatorMap[current] };
                _position++;
                return;
            }

            throw new Exception($"Unexpected character in operator: {_input[_position]}");
        }

        // Method to parse a word in the input string
        private void ParseWord()
        {
            var start = _position;

            while (_position < _input.Length && (char.IsLetter(_input[_position]) || char.IsDigit(_input[_position])))
            {
                _position++;
            }

            var word = _input.Substring(start, _position - start);

            switch (word)
            {
                case "true":
                    CurrentToken = new Token { Type = TokenType.True, BooleanValue = true };
                    break;
                case "false":
                    CurrentToken = new Token { Type = TokenType.False, BooleanValue = false };
                    break;
                case "and":
                    CurrentToken = new Token { Type = TokenType.And };
                    break;
                case "or":
                    CurrentToken = new Token { Type = TokenType.Or };
                    break;
                case "print":
                    CurrentToken = new Token { Type = TokenType.Print };
                    break;
                default:
                    CurrentToken = new Token { Type = TokenType.Identifier, StringValue = word };
                    break;
            }
        }

        // Method to parse a string in the input string
        private void ParseString()
        {
            _position++;

            var start = _position;
            while (_position < _input.Length && _input[_position] != '"')
            {
                _position++;
            }

            if (_position >= _input.Length)
            {
                throw new Exception("Unterminated string literal");
            }

            var str = _input.Substring(start, _position - start);
            _position++;

            CurrentToken = new Token { Type = TokenType.String, StringValue = str };
        }

        // Method to skip white spaces in the input string
        private void SkipWhiteSpace()
        {
            while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
            {
                _position++;
            }
        }
    }
}

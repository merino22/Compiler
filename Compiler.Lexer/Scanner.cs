using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Core.Enum;
using Compiler.Core.Interfaces;
using Compiler.Core.Models;
using Compiler.Core.Models.Lexer;

namespace Compiler.Lexer
{
    public class Scanner : IScanner
    {
        private Input _input;
        private readonly Dictionary<string, TokenType> _keywords;

        public Scanner(Input input)
        {
            this._input = input;
            this._keywords = new Dictionary<string, TokenType>
            {
                { "if", TokenType.IfKeyword  },
                { "foreach", TokenType.ForEachKeyword },
                { "while", TokenType.WhileKeyword },
                { "class", TokenType.ClassKeyword },
                { "for", TokenType.ForKeyword  },
                { "in", TokenType.InKeyword  },
                { "else", TokenType.ElseKeyword },
                { "int", TokenType.IntKeyword },
                { "float", TokenType.FloatKeyword},
                { "bool", TokenType.BoolKeyword },
                { "Date", TokenType.DateTimeKeyword },
                { "string", TokenType.StringKeyword }
            };
            //this.symbolTable = new List<Symbol>();
        }

        public Token GetNextToken()
        {
            var lexeme = new StringBuilder();
            var currentChar = GetNextChar();
            while (true)
            {
                while (char.IsWhiteSpace(currentChar) || currentChar == '\n')
                {
                    currentChar = GetNextChar();
                }
                if (char.IsLetter(currentChar))
                {
                    lexeme.Append(currentChar);
                    currentChar = PeekNextChar();
                    while (char.IsLetterOrDigit(currentChar))
                    {
                        currentChar = GetNextChar();
                        lexeme.Append(currentChar);
                        currentChar = PeekNextChar();
                    }

                    if (this._keywords.ContainsKey(lexeme.ToString()))
                    {
                        return new Token
                        {
                            TokenType = this._keywords[lexeme.ToString()],
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    }

                    return new Token
                    {
                        TokenType = TokenType.Identifier,
                        Column = _input.Position.Column,
                        Line = _input.Position.Line,
                        Lexeme = lexeme.ToString(),
                    };
                }
                else if (char.IsDigit(currentChar))
                {
                    lexeme.Append(currentChar);
                    currentChar = PeekNextChar();
                    while (char.IsDigit(currentChar))
                    {
                        currentChar = GetNextChar();
                        lexeme.Append(currentChar);
                        currentChar = PeekNextChar();
                    }

                    if (currentChar != '.' && currentChar != '/')
                    {
                        return new Token
                        {
                            TokenType = TokenType.IntConstant,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString(),
                        };
                    }

                    if(currentChar == '.')
                    {
                        currentChar = GetNextChar();
                        lexeme.Append(currentChar);
                        currentChar = PeekNextChar();

                        while (char.IsDigit(currentChar))
                        {
                            currentChar = GetNextChar();
                            lexeme.Append(currentChar);
                            currentChar = PeekNextChar();
                        }

                        return new Token
                        {
                            TokenType = TokenType.FloatConstant,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString(),
                        };
                    }

                    while (char.IsDigit(currentChar) || currentChar == '/')
                    {
                        currentChar = GetNextChar();
                        lexeme.Append(currentChar);
                        currentChar = PeekNextChar();
                    }

                    return new Token
                    {
                        TokenType = TokenType.DateConstant,
                        Column = _input.Position.Column,
                        Line = _input.Position.Line,
                        Lexeme = lexeme.ToString(),
                    };


                }
                else switch (currentChar)
                {
                    case '/':
                    {
                        currentChar = PeekNextChar();
                        if (currentChar != '*')
                        {
                            lexeme.Append(currentChar);
                            return new Token
                            {
                                TokenType = TokenType.Division,
                                Column = _input.Position.Column,
                                Line = _input.Position.Line,
                                Lexeme = lexeme.ToString()
                            };
                        }
                        while (true)
                        {
                            currentChar = GetNextChar();
                            while (currentChar == '*')
                            {
                                currentChar = GetNextChar();
                            }

                            if (currentChar == '/')
                            {
                                currentChar = GetNextChar();
                                break;
                            }
                        }
                        break;
                    }
                    case '<':
                        lexeme.Append(currentChar);
                        var nextChar = PeekNextChar();
                        switch (nextChar)
                        {
                            case '=':
                                GetNextChar();
                                lexeme.Append(nextChar);
                                return new Token
                                {
                                    TokenType = TokenType.LessOrEqualThan,
                                    Column = _input.Position.Column,
                                    Line = _input.Position.Line,
                                    Lexeme = lexeme.ToString()
                                };
                            case '>':
                                lexeme.Append(nextChar);
                                currentChar = GetNextChar();
                                return new Token
                                {
                                    TokenType = TokenType.NotEqual,
                                    Column = _input.Position.Column,
                                    Line = _input.Position.Line,
                                    Lexeme = lexeme.ToString()
                                };
                            default:
                                lexeme.Append(nextChar);
                                return new Token
                                {
                                    TokenType = TokenType.LessThan,
                                    Column = _input.Position.Column,
                                    Line = _input.Position.Line,
                                    Lexeme = lexeme.ToString().Trim()
                                };
                        }
                    case '>':
                        lexeme.Append(currentChar);
                        nextChar = PeekNextChar();
                        if (nextChar != '=')
                        {
                            return new Token
                            {
                                TokenType = TokenType.GreaterThan,
                                Column = _input.Position.Column,
                                Line = _input.Position.Line,
                                Lexeme = lexeme.ToString().Trim()
                            };
                        }

                        lexeme.Append(nextChar);
                        GetNextChar();
                        return new Token
                        {
                            TokenType = TokenType.GreaterOrEqualThan,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString().Trim()
                        };
                    case '+':
                        lexeme.Append(currentChar);
                        nextChar = PeekNextChar();
                        if (nextChar != '+')
                        {
                            return new Token
                            {
                                TokenType = TokenType.Plus,
                                Column = _input.Position.Column,
                                Line = _input.Position.Line,
                                Lexeme = lexeme.ToString().Trim()
                            };
                        }

                        lexeme.Append(nextChar);
                        GetNextChar();
                        return new Token
                        {
                            TokenType = TokenType.Increment,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString().Trim()
                        };

                    case '-':
                        lexeme.Append(currentChar);
                        nextChar = PeekNextChar();
                        if (nextChar != '-')
                        {
                            return new Token
                            {
                                TokenType = TokenType.Minus,
                                Column = _input.Position.Column,
                                Line = _input.Position.Line,
                                Lexeme = lexeme.ToString().Trim()
                            };
                        }

                        lexeme.Append(nextChar);
                        GetNextChar();
                        return new Token
                        {
                            TokenType = TokenType.Decrement,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString().Trim()
                        };
                    case '(':
                        lexeme.Append(currentChar);
                        return new Token
                        {
                            TokenType = TokenType.LeftParens,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    case ')':
                        lexeme.Append(currentChar);
                        return new Token
                        {
                            TokenType = TokenType.RightParens,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    case '*':
                        lexeme.Append(currentChar);
                        return new Token
                        {
                            TokenType = TokenType.Asterisk,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    case ';':
                        lexeme.Append(currentChar);
                        return new Token
                        {
                            TokenType = TokenType.SemiColon,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    case '=':
                        lexeme.Append(currentChar);
                        return new Token
                        {
                            TokenType = TokenType.Equal,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    case ':':
                    {
                        lexeme.Append(currentChar);
                        currentChar = GetNextChar();
                        if (currentChar != '=')
                        {
                            throw new ApplicationException($"Caracter {lexeme} invalido en la columna: {_input.Position.Column}, fila: {_input.Position.Line}");
                        }
                        lexeme.Append(currentChar);
                        return new Token
                        {
                            TokenType = TokenType.Assignation,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    }
                    case '\'':
                    {
                        lexeme.Append(currentChar);
                        currentChar = GetNextChar();
                        while (currentChar != '\'')
                        {
                            lexeme.Append(currentChar);
                            currentChar = GetNextChar();
                        }
                        lexeme.Append(currentChar);
                        return new Token
                        {
                            TokenType = TokenType.StringConstant,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    }
                    case '\0':
                        return new Token
                        {
                            TokenType = TokenType.EOF,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = string.Empty
                        };
                    case '{':
                        lexeme.Append(currentChar);
                        return new Token
                        {
                            TokenType = TokenType.OpenBrace,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    case '}':
                        lexeme.Append(currentChar);
                        return new Token
                        {
                            TokenType = TokenType.CloseBrace,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    case ',':
                        lexeme.Append(currentChar);
                        return new Token
                        {
                            TokenType = TokenType.Comma,
                            Column = _input.Position.Column,
                            Line = _input.Position.Line,
                            Lexeme = lexeme.ToString()
                        };
                    default:
                        throw new ApplicationException($"Caracter {lexeme} invalido en la columna: {_input.Position.Column}, fila: {_input.Position.Line}");
                }
            }
        }

        private char GetNextChar()
        {
            var next = _input.NextChar();
            _input = next.Reminder;
            return next.Value;
        }

        private char PeekNextChar()
        {
            var next = _input.NextChar();
            return next.Value;
        }
    }
}
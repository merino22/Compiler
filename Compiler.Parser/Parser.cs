using Compiler.Core;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Compiler.Core.Statements;
using System;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using Compiler.Core.Enum;
using Compiler.Core.Models;
using Compiler.Core.Models.Lexer;
using Compiler.Core.Models.Parser;
using Constant = Compiler.Core.Expressions.Constant;
using Environment = Compiler.Core.Models.Parser.Environment;
using Expression = Compiler.Core.Expressions.Expression;
using Type = Compiler.Core.Models.Parser.Type;

namespace Compiler.Parser
{
    public class Parser : IParser
    {
        private readonly IScanner scanner;
        private Token lookAhead;
        private Environment top;

        public Parser(IScanner scanner)
        {
            this.scanner = scanner;
            this.Move();
        }

        public Statement Parse()
        {
            return Program();
        }

        private Statement Program()
        {
            top = new Environment(top);
            top.AddMethod("print", new Id(new Token
            {
                Lexeme = "print"
            }, Type.Void),
            new ArgumentExpression(new Token
            {
                Lexeme = ","
            },
            new Id(new Token
            {
                Lexeme = "arg1"
            }, Type.String),
            new Id(new Token
            {
                Lexeme = "arg2"
            }, Type.String)));
            return Block();
        }

        private Statement Block()
        {
            Match(TokenType.OpenBrace);
            var previousSavedEnvironment = top;
            top = new Environment(top);
            Decls();
            var statements = Stmts();
            Match(TokenType.CloseBrace);
            top = previousSavedEnvironment;
            return statements;
        }

        private Statement Stmts()
        {
            if (this.lookAhead.TokenType == TokenType.CloseBrace)
            {//{}
                return null;
            }
            return new SequenceStatement(Stmt(), Stmts());
        }

        private Statement Stmt()
        {
            Expression expression;
            Statement statement1, statement2;
            switch (this.lookAhead.TokenType)
            {
                case TokenType.Identifier:
                    {
                        var symbol = top.Get(this.lookAhead.Lexeme);
                        Match(TokenType.Identifier);
                        if (this.lookAhead.TokenType == TokenType.Assignation)
                        {
                            return AssignStmt(symbol.Id);
                        }
                        else if (this.lookAhead.TokenType == TokenType.Increment)
                        {
                            return IncrementStmt(symbol.Id);
                        }
                        else if (this.lookAhead.TokenType == TokenType.Decrement)
                        {
                            return DecrementStmt(symbol.Id);
                        }
                        return CallStmt(symbol);
                    }
                case TokenType.IfKeyword:
                    {
                        Match(TokenType.IfKeyword);
                        Match(TokenType.LeftParens);
                        expression = Eq();
                        Match(TokenType.RightParens);
                        statement1 = Stmt();
                        if (this.lookAhead.TokenType != TokenType.ElseKeyword)
                        {
                            return new IfStatement(expression as TypedExpression, statement1);
                        }
                        Match(TokenType.ElseKeyword);
                        statement2 = Stmt();
                        return new ElseStatement(expression as TypedExpression, statement1, statement2);
                    }
                case TokenType.ForEachKeyword:
                {
                    Match(TokenType.ForEachKeyword);
                    Match(TokenType.LeftParens);
                    expression = Eq();
                    Match(TokenType.RightParens);
                    statement1 = Stmt();
                    return new ForEachStatement(expression, statement1);
                }
                case TokenType.WhileKeyword:
                {
                    Match(TokenType.WhileKeyword);
                    Match(TokenType.LeftParens);
                    expression = Eq();
                    Match(TokenType.RightParens);
                    statement1 = Stmt();
                    return new WhileStatement(expression, statement1);
                }
                default:
                    return Block();
            }
        }

        private Expression Eq()
        {
            var expression = Rel();
            while (this.lookAhead.TokenType == TokenType.Equal || this.lookAhead.TokenType == TokenType.NotEqual)
            {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Rel() as TypedExpression);
            }

            return expression;
        }

        private Expression Rel()
        {
            var expression = Expr();
            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan
                || this.lookAhead.TokenType == TokenType.LessOrEqualThan
                || this.lookAhead.TokenType == TokenType.GreaterOrEqualThan
                || this.lookAhead.TokenType == TokenType.InKeyword)
            {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Expr() as TypedExpression);
            }
            return expression;
        }

        private Expression Expr()
        {
            var expression = Term();
            while (this.lookAhead.TokenType == TokenType.Plus || this.lookAhead.TokenType == TokenType.Minus)
            {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Term() as TypedExpression);
            }
            return expression;
        }

        private Expression Term()
        {
            var expression = Factor();
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Division)
            {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Factor() as TypedExpression);
            }
            return expression;
        }

        private Expression Factor()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                        Match(TokenType.LeftParens);
                        var expression = Eq();
                        Match(TokenType.RightParens);
                        return expression;
                    }
                case TokenType.IntConstant:
                    var constant = new Constant(lookAhead, Type.Int);
                    Match(TokenType.IntConstant);
                    return constant;
                case TokenType.FloatConstant:
                    constant = new Constant(lookAhead, Type.Float);
                    Match(TokenType.FloatConstant);
                    return constant;
                case TokenType.StringConstant:
                    constant = new Constant(lookAhead, Type.String);
                    Match(TokenType.StringConstant);
                    return constant;
                case TokenType.Increment:
                    constant = new Constant(lookAhead, Type.Int);
                    Match(TokenType.Increment);
                    return constant;
                case TokenType.Decrement:
                    constant = new Constant(lookAhead, Type.Int);
                    Match(TokenType.Decrement);
                    return constant;
                default:
                    var symbol = top.Get(this.lookAhead.Lexeme);
                    Match(TokenType.Identifier);
                    return symbol.Id;
            }
        }

        private Statement CallStmt(Symbol symbol)
        {
            Match(TokenType.LeftParens);
            var @params = OptParams();
            Match(TokenType.RightParens);
            Match(TokenType.SemiColon);
            return new CallStatement(symbol.Id, @params, symbol.Attributes);
        }

        private Expression OptParams()
        {
            if (this.lookAhead.TokenType != TokenType.RightParens)
            {
                return Params();
            }
            return null;
        }

        private Expression Params()
        {
            var expression = Eq();
            if (this.lookAhead.TokenType != TokenType.Comma)
            {
                return expression;
            }
            Match(TokenType.Comma);
            expression = new ArgumentExpression(lookAhead, expression as TypedExpression, Params() as TypedExpression);
            return expression;
        }

        private Statement AssignStmt(Id id)
        {
            Match(TokenType.Assignation);
            var expression = Eq();
            Match(TokenType.SemiColon);
            return new AssignationStatement(id, expression as TypedExpression);
        }


        private Statement IncrementStmt(Id id)
        {
            //Match(TokenType.Increment);
            var expression = Eq();
            Match(TokenType.SemiColon);
            return new IncrementStatement(id, expression);
        }
        private Statement DecrementStmt(Id id)
        {
            //Match(TokenType.Decrement);
            var expression = Eq();
            Match(TokenType.SemiColon);
            return new DecrementStatement(id, expression);
        }
        private void Decls()
        {
            if (this.lookAhead.TokenType == TokenType.IntKeyword ||
                this.lookAhead.TokenType == TokenType.FloatKeyword ||
                this.lookAhead.TokenType == TokenType.StringKeyword)
            {
                Decl();
                Decls();
            }
        }

        private void Decl()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Float);
                    top.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.String);
                    top.AddVariable(token.Lexeme, id);
                    break;
                default:
                    Match(TokenType.IntKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Int);
                    top.AddVariable(token.Lexeme, id);
                    break;
            }
        }

        private void Move()
        {
            this.lookAhead = this.scanner.GetNextToken();
        }

        private void Match(TokenType tokenType)
        {
            if (this.lookAhead.TokenType != tokenType)
            {
                throw new ApplicationException($"Syntax error! expected token {tokenType} but found {this.lookAhead.TokenType}. Line: {this.lookAhead.Line}, Column: {this.lookAhead.Column}");
            }
            this.Move();
        }
    }
}

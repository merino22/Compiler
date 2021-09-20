using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Compiler.Core.Statements;
using System;
using Compiler.Core.Enum;
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
        private readonly IScanner _scanner;
        private Token _lookAhead;
        public Parser(IScanner scanner)
        {
            this._scanner = scanner;
            this.Move();
        }

        public Statement Parse()
        {
            return Program();
        }

        private Statement Program()
        {
            EnvironmentManager.PushContext();
            var block = Block();
            block.ValidateSemantic();
            var code = block.Generate(0);
            Console.WriteLine(code);
            //code = code.Replace($"else:{Environment.NewLine}\tif", "elif");
            return block;
        }
        private Statement Block()
        {
            if (this._lookAhead.TokenType == TokenType.ClassKeyword)
            {
                Match(TokenType.ClassKeyword);
                var token = this._lookAhead;
                Match(TokenType.Identifier);
                EnvironmentManager.AddMethod("class " + token.Lexeme, new Id(token
                    , Type.Class), null);
                Console.WriteLine("class " + token.Lexeme +"{}");
            }
            else if (this._lookAhead.TokenType == TokenType.FunctionKeyword)
            {
                Match(TokenType.FunctionKeyword);
                var token = this._lookAhead;
                Match(TokenType.Identifier);
                EnvironmentManager.AddMethod("void" + token.Lexeme, new Id(token
                    , Type.Func), null);
                Console.WriteLine("function " + token.Lexeme + "(){}");
            }

            if (this._lookAhead.TokenType == TokenType.FunctionKeyword ||
                this._lookAhead.TokenType == TokenType.ClassKeyword )
            {
                return Block();
            }
            Match(TokenType.OpenBrace);
            EnvironmentManager.PushContext();
            Decls();
            var statements = Stmts();
            Match(TokenType.CloseBrace);
            EnvironmentManager.PopContext();
            return statements;
        }

        private Statement Stmts()
        {
            if (this._lookAhead.TokenType == TokenType.CloseBrace)
            {//{}
                return null;
            }
            return new SequenceStatement(Stmt(), Stmts());
        }

        private Statement Stmt()
        {
            Expression expression;
            Statement statement1, statement2;
            switch (this._lookAhead.TokenType)
            {
                case TokenType.Identifier:
                    {
                        var symbol = EnvironmentManager.GetSymbol(this._lookAhead.Lexeme);
                        Match(TokenType.Identifier);
                        if (this._lookAhead.TokenType == TokenType.Assignation)
                        {
                            return AssignStmt(symbol.Id);
                        }
                        else if (this._lookAhead.TokenType == TokenType.Increment)
                        {
                            return IncrementStmt(symbol.Id);
                        }
                        else if (this._lookAhead.TokenType == TokenType.Decrement)
                        {
                            return DecrementStmt(symbol.Id);
                        }
                        return CallStmt(symbol);
                    }
                case TokenType.IfKeyword:
                    {
                        Match(TokenType.IfKeyword);
                        Match(TokenType.LeftParens);
                        if(this._lookAhead.TokenType == TokenType.Not)
                        {
                            Logic(null);
                        }
                        expression = Eq();
                        if(this._lookAhead.TokenType == TokenType.And || this._lookAhead.TokenType == TokenType.Or)
                        {
                            expression = Logic(expression as TypedExpression);
                        }
                        Match(TokenType.RightParens);
                        statement1 = Stmt();
                        if (this._lookAhead.TokenType != TokenType.ElseKeyword)
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
                    return new ForEachStatement(expression as TypedExpression, statement1);
                }
                case TokenType.WhileKeyword:
                {
                    Match(TokenType.WhileKeyword);
                    Match(TokenType.LeftParens);
                    expression = Eq();
                    if (this._lookAhead.TokenType == TokenType.And || this._lookAhead.TokenType == TokenType.Or)
                    {
                        expression = Logic(expression as TypedExpression);
                    }
                    Match(TokenType.RightParens);
                    statement1 = Stmt();
                    return new WhileStatement(expression as TypedExpression, statement1);
                }
                default:
                    return Block();
            }
        }

        private Expression Eq()
        {
            var expression = Rel();
            while (this._lookAhead.TokenType == TokenType.Equal || this._lookAhead.TokenType == TokenType.NotEqual)
            {
                var token = _lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Rel() as TypedExpression);
            }
            return expression;
        }

        private Expression Rel()
        {
            var expression = Expr();
            if (this._lookAhead.TokenType == TokenType.LessThan
                || this._lookAhead.TokenType == TokenType.GreaterThan
                || this._lookAhead.TokenType == TokenType.LessOrEqualThan
                || this._lookAhead.TokenType == TokenType.GreaterOrEqualThan)
            {
                var token = _lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Expr() as TypedExpression);
            }

            if (this._lookAhead.TokenType == TokenType.InKeyword)
            {
                var token = _lookAhead;
                Move();
                Match(TokenType.Identifier);
                expression = new ArgumentExpression(token, expression as TypedExpression, null);
            }
            return expression;
        }

        private Expression Logic(TypedExpression expr)
        {
            var expression = Rel();
            if (_lookAhead.TokenType == TokenType.And
             || _lookAhead.TokenType == TokenType.Or)
            {
                var token = _lookAhead;
                Move();
                if (expr != null)
                {
                    expression = expr;
                }
                expression = new LogicalExpression(token, expression as TypedExpression, Logic(null) as TypedExpression);
            }

            if (_lookAhead.TokenType == TokenType.Not)
            {
                var token = _lookAhead;
                Move();
                expression = new LogicalExpression(token, expression as TypedExpression, null);
            }
            return expression;
        }

        private Expression Expr()
        {
            var expression = Term();
            while (this._lookAhead.TokenType == TokenType.Plus || this._lookAhead.TokenType == TokenType.Minus)
            {
                var token = _lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Term() as TypedExpression);
            }
            return expression;
        }

        private Expression Term()
        {
            var expression = Factor();
            while (this._lookAhead.TokenType == TokenType.Asterisk || this._lookAhead.TokenType == TokenType.Division)
            {
                var token = _lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Factor() as TypedExpression);
            }
            return expression;
        }

        private Expression Factor()
        {
            switch (this._lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                        Match(TokenType.LeftParens);
                        var expression = Eq();
                        Match(TokenType.RightParens);
                        return expression;
                    }
                case TokenType.IntConstant:
                    var constant = new Constant(_lookAhead, Type.Int);
                    Match(TokenType.IntConstant);
                    return constant;
                case TokenType.FloatConstant:
                    constant = new Constant(_lookAhead, Type.Float);
                    Match(TokenType.FloatConstant);
                    return constant;
                case TokenType.StringConstant:
                    constant = new Constant(_lookAhead, Type.String);
                    Match(TokenType.StringConstant);
                    return constant;
                case TokenType.Increment:
                    constant = new Constant(_lookAhead, Type.Int);
                    Match(TokenType.Increment);
                    return constant;
                case TokenType.Decrement:
                    constant = new Constant(_lookAhead, Type.Int);
                    Match(TokenType.Decrement);
                    return constant;
                case TokenType.DateConstant:
                    constant = new Constant(_lookAhead, Type.Date);
                    Match(TokenType.DateConstant);
                    return constant;
                case TokenType.And:
                    return null;
                case TokenType.Or:
                    return null;
                case TokenType.Not:
                    return null;
                default:
                    var symbol = EnvironmentManager.GetSymbol(this._lookAhead.Lexeme);
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
            if (this._lookAhead.TokenType != TokenType.RightParens)
            {
                return Params();
            }
            return null;
        }

        private Expression Params()
        {
            var expression = Eq();
            if (this._lookAhead.TokenType != TokenType.Comma)
            {
                return expression;
            }
            Match(TokenType.Comma);
            expression = new ArgumentExpression(_lookAhead, expression as TypedExpression, Params() as TypedExpression);
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
            //Console.WriteLine(Eq().Generate());
            return new IncrementStatement(id, expression as TypedExpression);
        }
        private Statement DecrementStmt(Id id)
        {
            //Match(TokenType.Decrement);
            var expression = Eq();
            Match(TokenType.SemiColon);
            return new DecrementStatement(id, expression as TypedExpression);
        }
        private void Decls()
        {
            if (this._lookAhead.TokenType == TokenType.IntKeyword ||
                this._lookAhead.TokenType == TokenType.FloatKeyword ||
                this._lookAhead.TokenType == TokenType.StringKeyword ||
                this._lookAhead.TokenType == TokenType.DateTimeKeyword)
            {
                Decl();
                Decls();
            }
        }

        private void Decl()
        {
            switch (this._lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    var token = this._lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Float);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    token = this._lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.String);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.DateTimeKeyword:
                    Match(TokenType.DateTimeKeyword);
                    token = this._lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Date);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.BoolKeyword:
                    Match(TokenType.BoolKeyword);
                    token = this._lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Bool);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                default:
                    Match(TokenType.IntKeyword);
                    token = this._lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Int);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
            }
        }

        private void Move()
        {
            this._lookAhead = this._scanner.GetNextToken();
        }

        private void Match(TokenType tokenType)
        {
            if (this._lookAhead.TokenType != tokenType)
            {
                throw new ApplicationException($"Syntax error! expected token {tokenType} but found {this._lookAhead.TokenType}. Line: {this._lookAhead.Line}, Column: {this._lookAhead.Column}");
            }
            this.Move();
        }
    }
}

using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Compiler.Core.Statements;
using System;
using System.Collections.Generic;
using Compiler.Core.Enum;
using Compiler.Core.Models.Lexer;
using Compiler.Core.Models.Parser;
using Constant = Compiler.Core.Expressions.Constant;
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
            block.Interpret();
            var code = block.Generate(0);
            System.IO.File.WriteAllText(@"C:\Users\Public\code.js", code);
            Console.WriteLine(code);
            return block;
        }
        private Statement Block()
        {
            switch (this._lookAhead.TokenType)
            {
                case TokenType.ClassKeyword:
                    Match(TokenType.ClassKeyword);
                    var token = this._lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.OpenBrace);
                    EnvironmentManager.PushContext();
                    Decls();
                    var statementsClass = Stmts();
                    Match(TokenType.CloseBrace);
                    EnvironmentManager.PopContext();
                    return new ClassStatement(statementsClass, token);
                case TokenType.FunctionKeyword:
                    Match(TokenType.FunctionKeyword);
                    token = this._lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.LeftParens);
                    EnvironmentManager.PushContext();
                    var argumentList = new List<Token>();
                    while (_lookAhead.TokenType==TokenType.Identifier)
                    {
                        token = _lookAhead;
                        argumentList.Add(token);
                        Match(TokenType.Identifier);
                        if (_lookAhead.TokenType == TokenType.Comma)
                        {
                            Match(TokenType.Comma);
                        }
                        var id = new Id(token, Type.Void);
                        EnvironmentManager.AddVariable(token.Lexeme, id);
                    }
                    Match(TokenType.RightParens);
                    Match(TokenType.OpenBrace);
                    Decls();
                    var statementsFunctions = Stmts();
                    Match(TokenType.CloseBrace);
                    EnvironmentManager.PopContext();
                    return new FunctionStatement(statementsFunctions, token, argumentList);
                default:
                    Match(TokenType.OpenBrace);
                    EnvironmentManager.PushContext();
                    Decls();
                    var statements = Stmts();
                    Match(TokenType.CloseBrace);
                    EnvironmentManager.PopContext();
                    return statements;
            }
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
            Decls();
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
                        var tokentype = _lookAhead;
                        if (tokentype.TokenType== TokenType.Not)
                        {
                            Match(TokenType.Not);
                        }
                        expression = Eq();
                        if (tokentype.TokenType == TokenType.Not)
                        {
                            NotLogic(tokentype,expression as TypedExpression);
                        }
                        if (this._lookAhead.TokenType == TokenType.And || this._lookAhead.TokenType == TokenType.Or)
                        {
                            Move();
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
                    var token1 = _lookAhead;
                    Match(TokenType.Identifier);
                    var id1 = new Id(token1, Type.Void);
                    EnvironmentManager.AddVariable(token1.Lexeme, id1);

                    Match(TokenType.InKeyword);
                    var token2 = _lookAhead;
                    Match(TokenType.Identifier);
                    if (EnvironmentManager.GetSymbolForEvaluation(token2.Lexeme) == null)
                    {
                        throw new ApplicationException($"Variable {token2.Lexeme} Doesn't Exist");
                    }
                    else if(EnvironmentManager.GetSymbolForEvaluation(token2.Lexeme).Id.Type != Type.IntList)
                    {
                        throw new ApplicationException($"Variable {token2.Lexeme} Is not a List");
                    }
                    Match(TokenType.RightParens);
                    statement1 = Stmt();
                    return new ForeachStatement(token1, token2, statement1);
                }
                case TokenType.WhileKeyword:
                {
                    Match(TokenType.WhileKeyword);
                    Match(TokenType.LeftParens);
                    expression = Eq();
                    if (this._lookAhead.TokenType == TokenType.And || this._lookAhead.TokenType == TokenType.Or)
                    {
                        Move();
                        expression = Logic(expression as TypedExpression);
                    }
                    Match(TokenType.RightParens);
                    statement1 = Stmt();
                    return new WhileStatement(expression as TypedExpression, statement1);
                }
                case TokenType.ConsoleKeyword:
                    Match(TokenType.ConsoleKeyword);
                    return ConsoleStatement();
                default:
                    return Block();
            }
        }

        private Statement ConsoleStatement()
        {
            Match(TokenType.Decimal);
            if (_lookAhead.TokenType == TokenType.WriteLineKeyword)
            {
                Match(TokenType.WriteLineKeyword);
                Match(TokenType.LeftParens);
                var token = this._lookAhead;
                Match(TokenType.Identifier);
                Match(TokenType.RightParens);
                Match(TokenType.SemiColon);
                return new WriteLineStatement(token);
            }
            else if (_lookAhead.TokenType == TokenType.ReadLineKeyword)
            {
                Match(TokenType.ReadLineKeyword);
                Match(TokenType.LeftParens);
                var token = _lookAhead;
                Match(TokenType.Identifier);
                Match(TokenType.RightParens);
                Match(TokenType.SemiColon);
                return new ReadLineStatement(token);
            }
            return null;
        }
        private Statement IncrementStmt(Id id)
        {
            var token = _lookAhead;
            Match(TokenType.Increment);
            Match(TokenType.SemiColon);
            return new IncrementStatement(id, token);
        }
        private Statement DecrementStmt(Id id)
        {
            var token = _lookAhead;
            Match(TokenType.Decrement);
            Match(TokenType.SemiColon);
            return new DecrementStatement(id, token);
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
            return expression;
        }

        private Expression NotLogic(Token token, TypedExpression expr)
        {
            var expression = expr;
            return new LogicalExpression(token, expression as TypedExpression, null);
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
                case TokenType.IntListConstant:
                    constant = new Constant(_lookAhead, Type.IntList);
                    Match(TokenType.IntListKeyword);
                    return constant;
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


        private void Decls()
        {
            if (this._lookAhead.TokenType == TokenType.IntKeyword ||
                this._lookAhead.TokenType == TokenType.FloatKeyword ||
                this._lookAhead.TokenType == TokenType.StringKeyword ||
                this._lookAhead.TokenType == TokenType.DateTimeKeyword ||
                this._lookAhead.TokenType == TokenType.IntListKeyword)
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
                case TokenType.IntListKeyword:
                    Match(TokenType.IntListKeyword);
                    token = this._lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.IntList);
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

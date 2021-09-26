using Compiler.Core.Enum;
using Compiler.Core.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Type = Compiler.Core.Models.Parser.Type;

namespace Compiler.Core.Expressions
{
    public class LogicalExpression : TypedBinaryOperator
    {
        private readonly Dictionary<(Type, Type), Type> _typeRules;
        public LogicalExpression(Token token, TypedExpression leftExpression, TypedExpression rightExpression)
            : base(token, leftExpression, rightExpression, null)
        {
            _typeRules = new Dictionary<(Type, Type), Type>
            {
                { (Type.Float, Type.Float), Type.Float },
                { (Type.Int, Type.Int), Type.Int },
                { (Type.String, Type.String), Type.String },
                { (Type.Float, Type.Int), Type.Float },
                { (Type.Int, Type.Float), Type.Float },
                { (Type.String, Type.Int), Type.String  },
                { (Type.String, Type.Float), Type.String  },
                { (Type.Float, Type.String), Type.String},
                { (Type.Date, Type.Date), Type.Date},
                { (Type.Date, Type.Int), Type.Date}
            };
        }
        public override dynamic Evaluate()
        {
            return Token.TokenType switch
            {
                TokenType.And => LeftExpression.Evaluate() && RightExpression.Evaluate(),
                TokenType.Or => LeftExpression.Evaluate() || RightExpression.Evaluate(),
                TokenType.Not => LeftExpression.Token.TokenType == TokenType.NotEqual? (dynamic)(LeftExpression.Token.TokenType=TokenType.Equal) : LeftExpression.Token.TokenType = TokenType.NotEqual,
                _ => throw new NotImplementedException()
            };
        }

        public override string Generate()
        {
            return $"{LeftExpression?.Generate()} {Token?.Lexeme} {RightExpression?.Generate()}";
        }

        public override Type GetExpressionType()
        {
            return Type.Bool;
        }
    }
}

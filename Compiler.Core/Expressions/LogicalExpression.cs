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
                { (Type.Float, Type.Float), Type.Bool },
                { (Type.Int, Type.Int), Type.Bool },
                { (Type.String, Type.String), Type.Bool },
                { (Type.Float, Type.Int), Type.Bool },
                { (Type.Int, Type.Float), Type.Bool }
            };
        }
        public override dynamic Evaluate()
        {
            return Token.TokenType switch
            {
                TokenType.And => LeftExpression.Evaluate() && RightExpression.Evaluate(),
                TokenType.Or => LeftExpression.Evaluate() || RightExpression.Evaluate(),
                TokenType.Not => !(LeftExpression.Evaluate() && RightExpression.Evaluate()),
                _ => throw new NotImplementedException()
            };
        }

        public override string Generate()
        {
            throw new NotImplementedException();
        }

        public override Models.Parser.Type GetExpressionType()
        {
            throw new NotImplementedException();
        }
    }
}

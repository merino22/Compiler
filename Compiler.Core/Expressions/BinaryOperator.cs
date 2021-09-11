using System;
using Compiler.Core.Models;
using Type = Compiler.Core.Models.Parser.Type;

namespace Compiler.Core.Expressions
{
    public abstract class BinaryOperator : Expression
    {

        public BinaryOperator(Token token, TypedExpression leftExpression, TypedExpression rightExpression, Type type)
            : base(token, type)
        {
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
        }

        public TypedExpression LeftExpression { get; }
        public TypedExpression RightExpression { get; }
    }
}

using System;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;

namespace Compiler.Core.Statements
{
    public class WhileStatement : Statement, ISemanticValidation
    {
        public WhileStatement(Expression expression, TypedExpression statement)
        {
            Expression = expression;
            Statement = statement;
            ValidateSemantic();
        }

        public Expression Expression { get; }
        public TypedExpression Statement { get; }
        public override void Interpret()
        {
            if (Expression?.Evaluate())
            {

            }
        }

        public override void ValidateSemantic()
        {
            //Statement?.ValidateSemantic();
        }

        public override string Generate(int tabs)
        {
            return "while(){}";
        }
    }
}

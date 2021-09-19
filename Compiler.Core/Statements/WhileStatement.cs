using System;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;

namespace Compiler.Core.Statements
{
    public class WhileStatement : Statement, ISemanticValidation
    {
        public WhileStatement(Expression expression, Statement statement)
        {
            Expression = expression;
            Statement = statement;
            ValidateSemantic();
        }

        public Expression Expression { get; }
        public Statement Statement { get; }
        public override void Interpret()
        {
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

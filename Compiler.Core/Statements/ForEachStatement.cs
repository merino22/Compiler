using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;

namespace Compiler.Core.Statements
{
    public class ForEachStatement : Statement, ISemanticValidation
    {
        public ForEachStatement(TypedExpression expression, Statement statement)
        {
            Expression = expression;
            Statement = statement;
            ValidateSemantic();
        }

        public TypedExpression Expression { get; }
        public Statement Statement { get; }
        public override void Interpret()
        {
            if (Expression?.Evaluate())
            {

            }
        }

        public override void ValidateSemantic()
        {
        }

        public override string Generate(int tabs)
        {
            return "numbers.foreach(number=>{})";
        }
    }
}

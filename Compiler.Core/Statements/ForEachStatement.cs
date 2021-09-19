using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;

namespace Compiler.Core.Statements
{
    public class ForEachStatement : Statement, ISemanticValidation
    {
        public ForEachStatement(Expression expression, Statement statement)
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
        }

        public override string Generate(int tabs)
        {
            return "numbers.foreach(number=>{})";
        }
    }
}

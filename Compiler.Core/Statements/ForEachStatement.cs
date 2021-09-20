using System;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Type = Compiler.Core.Models.Parser.Type;

namespace Compiler.Core.Statements
{
    public class ForEachStatement : Statement, ISemanticValidation
    {
        public ForEachStatement(TypedExpression expression, Statement statement)
        {
            Expression = expression;
            Statement = statement;
        }

        public TypedExpression Expression { get; }
        public Statement Statement { get; }
        public override void Interpret()
        {
            if (Expression.Evaluate())
            {
                Statement.Interpret();
            }
        }

        public override void ValidateSemantic()
        {
            
        }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            code += $"foreach({Expression?.Evaluate()}){Environment.NewLine}{{{Statement.Generate(tabs)}}}";
            return code;
        }
    }
}

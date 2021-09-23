using System;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Type = Compiler.Core.Models.Parser.Type;

namespace Compiler.Core.Statements
{
    public class WhileStatement : Statement, ISemanticValidation
    {
        public WhileStatement(TypedExpression expression, Statement statement)
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
            if (Expression.GetExpressionType() != Type.Bool)
            {
                throw new ApplicationException("A boolean is required in whiles");
            }
        }

        public override string Generate(int tabs)
        {
            var code = "\n";
            code += GetCodeInit(tabs);
            code += $"while({Expression.Generate()}){{{Environment.NewLine}";
            code += $"{Statement.Generate(tabs + 1)}";
            for (int i = 0; i < tabs; i++)
            {
                code += "\t";
            }

            code += "}";
            return code;
        }
    }
}

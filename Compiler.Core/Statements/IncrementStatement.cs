using System;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Compiler.Core.Models.Parser;
using Environment = System.Environment;

namespace Compiler.Core.Statements
{
    public class IncrementStatement : Statement, ISemanticValidation
    {
        public IncrementStatement(Id id, TypedExpression expression)
        {
            Id = id;
            Expression = expression;
        }


        public Id Id { get; }
        public TypedExpression Expression { get; }
        public override void Interpret()
        {
            EnvironmentManager.UpdateVariable(Id.Token.Lexeme, Expression.Evaluate());
        }

        public override void ValidateSemantic()
        {
            if (Id.GetExpressionType() != Expression.Type)
            {
                throw new ApplicationException($"Type {Id.GetExpressionType()} is not assignable to {Expression.Type}");
            }
        }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            code += $"{Id.Generate()}++{Environment.NewLine}";
            return code;
        }
    }

}

﻿using System;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;

namespace Compiler.Core.Statements
{
    public class IncrementStatement : Statement, ISemanticValidation
    {
        public IncrementStatement(Id id, TypedExpression expression)
        {
            Id = id;
            Expression = expression;
            ValidateSemantic();
        }


        public Id Id { get; }
        public TypedExpression Expression { get; }
        public override void Interpret()
        {
            if (Expression?.Evaluate())
            {

            }
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
            return "";
        }
    }

}

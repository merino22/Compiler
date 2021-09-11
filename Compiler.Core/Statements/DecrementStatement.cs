﻿using System;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;

namespace Compiler.Core.Statements
{
    public class DecrementStatement: Statement, ISemanticValidation
    {
        public DecrementStatement(Id id, Expression expression)
        {
            Id = id;
            Expression = expression;
            ValidateSemantic();
        }


        public Id Id { get; }
        public Expression Expression { get; }

        public override void ValidateSemantic()
        {
            if (Id.GetExpressionType() != Expression.Type)
            {
                throw new ApplicationException($"Type {Id.GetExpressionType()} is not assignable to {Expression.Type}");
            }
        }
    }
}
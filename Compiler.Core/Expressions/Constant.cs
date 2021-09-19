using System;
using Compiler.Core.Enum;
using Compiler.Core.Models.Lexer;
using Type = Compiler.Core.Models.Parser.Type;

namespace Compiler.Core.Expressions
{
    public class Constant : TypedExpression
    {
        public Constant(Token token, Type type)
            : base(token, type)
        {
        }

        public override dynamic Evaluate()
        {
            return Token.TokenType switch
            {
                TokenType.IntConstant => Convert.ToInt32(Token.Lexeme),
                TokenType.FloatConstant => float.Parse(Token.Lexeme),
                TokenType.DateConstant => DateTime.Parse(Token.Lexeme),
                TokenType.BoolKeyword => Boolean.Parse(Token.Lexeme),
                TokenType.StringConstant => Token.Lexeme,
                _ => throw new NotImplementedException()
            };
        }

        public override Type GetExpressionType()
        {
            return Type;
        }

        public override string Generate()
        {
            return Evaluate().ToString();
        }
    }
}

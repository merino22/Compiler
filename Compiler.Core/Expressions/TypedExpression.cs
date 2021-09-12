using Compiler.Core.Models.Lexer;
using Compiler.Core.Models.Parser;

namespace Compiler.Core.Expressions
{
    public abstract class TypedExpression : Expression
    {
        public TypedExpression(Token token, Type type)
            : base(token, type)
        {
        }

        public abstract Type GetExpressionType();
    }
}

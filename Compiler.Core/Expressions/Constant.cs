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

        public override Type GetExpressionType()
        {
            return Type;
        }
    }
}

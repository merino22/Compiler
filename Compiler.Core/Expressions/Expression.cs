using Compiler.Core.Models.Lexer;
using Compiler.Core.Models.Parser;
using Type = Compiler.Core.Models.Parser.Type;

namespace Compiler.Core.Expressions
{
    public abstract class Expression : Node
    {
        public Type Type { get; }

        public Token Token { get; }

        public Expression(Token token, Type type)
        {
            Token = token;
            this.Type = type;
        }
        public abstract string Generate();
    }
}

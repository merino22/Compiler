using Compiler.Core.Models.Lexer;

namespace Compiler.Core.Interfaces
{
    public interface IResult
    {

        public abstract  Result<T> Empty<T>(Input reminder);

        public abstract  Result<T> Value<T>(T value, Input reminder);
    }
}
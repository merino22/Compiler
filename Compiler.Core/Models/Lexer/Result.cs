using Compiler.Core.Interfaces;

namespace Compiler.Core.Models.Lexer
{
    public class Result: IResult
    {
        Result<T> IResult.Value<T>(T value, Input reminder)
        {
            return new Result<T>(value, reminder);
        }

        Result<T> IResult.Empty<T>(Input reminder)
        {
            return new Result<T>(reminder);
        }
        
    }
}

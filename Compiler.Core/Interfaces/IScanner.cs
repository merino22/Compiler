using Compiler.Core.Models.Lexer;

namespace Compiler.Core.Interfaces
{
    public interface IScanner
    {
        Token GetNextToken();
    }
}
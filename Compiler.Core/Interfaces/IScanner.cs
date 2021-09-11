using Compiler.Core.Models;

namespace Compiler.Core.Interfaces
{
    public interface IScanner
    {
        Token GetNextToken();
    }
}
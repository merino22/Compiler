using Compiler.Core.Interfaces;
using Compiler.Core.Models.Parser;

namespace Compiler.Core.Statements
{
    public abstract class Statement : Node, ISemanticValidation
    {
        public abstract void ValidateSemantic();
    }
}

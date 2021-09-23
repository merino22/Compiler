using Compiler.Core.Interfaces;

namespace Compiler.Core.Statements
{
    public class ReadLineStatementL: Statement, ISemanticValidation
    {
        public ReadLineStatementL()
        {
            
        }
        public override void Interpret()
        {
            
        }

        public override void ValidateSemantic()
        {

        }

        public override string Generate(int tabs)
        {
            var code = "\n";
            code += GetCodeInit(tabs);
            code += "\n";
            return code;
        }
    }
}
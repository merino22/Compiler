using Compiler.Core.Interfaces;

namespace Compiler.Core.Statements
{
    public class WriteLineStatement: Statement, ISemanticValidation
    {
        public WriteLineStatement()
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
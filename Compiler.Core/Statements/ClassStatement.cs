using Compiler.Core.Interfaces;
using Compiler.Core.Models.Lexer;

namespace Compiler.Core.Statements
{
    public class ClassStatement: Statement, ISemanticValidation
    {
        public Statement Statement { get; }
        public Token Token { get; }

        public ClassStatement(Statement statement, Token token)
        {
            Statement = statement;
            Token = token;
        }
        public override void Interpret()
        {

        }

        public override void ValidateSemantic()
        {
            Statement?.ValidateSemantic();
        }

        public override string Generate(int tabs)
        {
            var code = "\n";
            code += GetCodeInit(tabs);
            code += $"class {Token.Lexeme}{{}}";
            code += Statement?.Generate(tabs);
            code += "\n";
            return code;
        }
    }
}
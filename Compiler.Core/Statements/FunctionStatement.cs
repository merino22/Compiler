using Compiler.Core.Interfaces;
using Compiler.Core.Models.Lexer;

namespace Compiler.Core.Statements
{
    public class FunctionStatement: Statement, ISemanticValidation
    {

        public Statement Statement { get; }
        public Token Token { get; }

        public FunctionStatement(Statement statement, Token token)
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
            code += $"function {Token.Lexeme}(){{";
            code += Statement?.Generate(tabs + 1);
            for (int i = 0; i < tabs; i++)
            {
                code += "\t";
            }
            code += "}";
            code += "\n";
            return code;
        }
    }
}
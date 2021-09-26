using System;
using Compiler.Core.Interfaces;
using Compiler.Core.Models.Lexer;
using Compiler.Core.Models.Parser;

namespace Compiler.Core.Statements
{
    public class WriteLineStatement: Statement, ISemanticValidation
    {
        public Token Token { get; }

        public WriteLineStatement(Token token)
        {
            Token = token;
        }
        public override void Interpret()
        {

        }

        public override void ValidateSemantic()
        {
            var symbol = EnvironmentManager.GetSymbolForEvaluation(Token.Lexeme);
            if (symbol == null)
            {
                throw new ApplicationException($"Variable doesn't exist");
            }
        }

        public override string Generate(int tabs)
        {
            var code = "\n";
            code += GetCodeInit(tabs);
            code += $"alert(`Input entered is: ${{{Token.Lexeme}}}`); ";
            code += "\n";
            return code;
        }
    }
}
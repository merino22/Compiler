using System;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Compiler.Core.Models.Lexer;
using Compiler.Core.Models.Parser;
using Environment = System.Environment;

namespace Compiler.Core.Statements
{
    public class DecrementStatement: Statement, ISemanticValidation
    {
        public Id Id { get; }
        public Token Token { get; }

        public DecrementStatement(Id id, Token token)
        {
            Id = id;
            Token = token;
        }
        public override void Interpret()
        {
            var symbol = EnvironmentManager.GetSymbolForEvaluation(Id.Token.Lexeme);
            EnvironmentManager.UpdateVariable(symbol.Id.Token.Lexeme, symbol.Value - 1);
        }

        public override void ValidateSemantic()
        {
            switch (Id.Type.Lexeme)
            {
                case "int":
                case "float":
                    break;
                default:
                    throw new ApplicationException($"Type {Id.Type} cannot be incremented");
            }
        }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            code += $"{Id.Token.Lexeme}{Token.Lexeme}\n";
            return code;
        }
    }
}

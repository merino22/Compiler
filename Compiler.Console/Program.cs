using System.IO;
using Compiler.Core.Engine;
using Compiler.Core.Models.Lexer;
using Compiler.Lexer;
using Environment = System.Environment;

namespace Compiler.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = File.ReadAllText("Code.txt").Replace(Environment.NewLine, "\n");
            var input = new Input(code);
            var scanner = new Scanner(input);
            var parser = new Parser.Parser(scanner);
            var engine = new CompilerEngine(parser);
            engine.Run();
            //System.Console.WriteLine("success${code}");
        }
    }
}

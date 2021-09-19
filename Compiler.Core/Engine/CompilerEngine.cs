using Compiler.Core.Interfaces;

namespace Compiler.Core.Engine
{
    public class CompilerEngine
    {
        private readonly IParser parser;

        public CompilerEngine(IParser parser)
        {
            this.parser = parser;
        }

        public void Run()
        {
            var intermediateCode = this.parser.Parse();
        }
    }
}
namespace Compiler.Core.Models.Lexer
{
    public readonly struct Input
    {
        public string Source { get; }

        public int Length { get; }

        public Position Position { get; }

        public Input(string source)
            : this(source, Position.Start, source.Length)
        {

        }
        public Input(string source, Position position, int length)
        {
            Source = source;
            Position = position;
            Length = length;
        }

        public Result<char> NextChar()
        {
            return Length == 0
                ? Result.Empty<char>(this)
                : Result.Value(Source[Position.Absolute], new Input(Source, Position.MovePointer(Source[Position.Absolute]), Length - 1));
        }

    }
}

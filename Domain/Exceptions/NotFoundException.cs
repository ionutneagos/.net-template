namespace Domain.Exceptions
{
    public sealed class NotFoundException : Exception
    {
        public int Code { get; } = 500;
        public NotFoundException()
        {

        }
        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}

namespace Domain.Exceptions
{
    public sealed class UnauthorizedException : Exception
    {
        public int Code { get; private set; }
        public UnauthorizedException()
        {

        }

        public UnauthorizedException(int code, string message)
            : base(message)
        {
            Code = code;
        }

        public UnauthorizedException(int code, string message, Exception inner)
        : base(message, inner)
        {
            Code = code;
        }
    }
}
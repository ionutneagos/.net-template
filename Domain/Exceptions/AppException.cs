namespace Domain.Exceptions
{
    public sealed class AppException : Exception
    {
        public int Code { get; private set; }
        public AppException()
        {

        }

        public AppException(int code, string message)
            : base(message)
        {
            Code = code;
        }

        public AppException(int code, string message, Exception inner)
        : base(message, inner)
        {
            Code = code;
        }
    }
}
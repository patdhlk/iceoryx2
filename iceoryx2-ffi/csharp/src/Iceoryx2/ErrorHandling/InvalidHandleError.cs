namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error caused by an invalid handle.
    /// </summary>
    public class InvalidHandleError : Iox2Error
    {
        public string? HandleType { get; }
        public override Iox2ErrorKind Kind => Iox2ErrorKind.InvalidHandle;
        public override string? Details { get; }
        public override string Message
        {
            get
            {
                var msg = HandleType != null
                    ? $"Invalid {HandleType} handle"
                    : "Invalid handle";
                return Details != null ? $"{msg}. Details: {Details}" : $"{msg}.";
            }
        }

        public InvalidHandleError(string? handleType = null, string? details = null)
        {
            HandleType = handleType;
            Details = details;
        }
    }
}
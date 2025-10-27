namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during request-response service creation.
    /// </summary>
    public class RequestResponseServiceCreationError : Iox2Error
    {
        public string? ServiceName { get; }
        public override Iox2ErrorKind Kind => Iox2ErrorKind.RequestResponseServiceCreationFailed;
        public override string? Details { get; }
        public override string Message
        {
            get
            {
                var msg = ServiceName != null
                    ? $"Failed to create request-response service '{ServiceName}'"
                    : "Failed to create request-response service";
                return Details != null ? $"{msg}. Details: {Details}" : $"{msg}.";
            }
        }

        public RequestResponseServiceCreationError(string? serviceName, string? details = null)
        {
            ServiceName = serviceName;
            Details = details;
        }
    }
}
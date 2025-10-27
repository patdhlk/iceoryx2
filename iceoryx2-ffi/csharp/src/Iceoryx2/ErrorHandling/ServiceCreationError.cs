namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during service creation.
    /// </summary>
    public class ServiceCreationError : Iox2Error
    {
        public string? ServiceName { get; }
        public override Iox2ErrorKind Kind => Iox2ErrorKind.ServiceCreationFailed;
        public override string? Details { get; }
        public override string Message
        {
            get
            {
                var msg = ServiceName != null
                    ? $"Failed to create service '{ServiceName}'"
                    : "Failed to create service";
                return Details != null ? $"{msg}. Details: {Details}" : $"{msg}.";
            }
        }

        public ServiceCreationError(string? serviceName, string? details = null)
        {
            ServiceName = serviceName;
            Details = details;
        }
    }
}
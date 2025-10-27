namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during event service creation.
    /// </summary>
    public class EventServiceCreationError : Iox2Error
    {
        public string? ServiceName { get; }
        public override Iox2ErrorKind Kind => Iox2ErrorKind.EventServiceCreationFailed;
        public override string? Details { get; }
        public override string Message
        {
            get
            {
                var msg = ServiceName != null
                    ? $"Failed to create event service '{ServiceName}'"
                    : "Failed to create event service";
                return Details != null ? $"{msg}. Details: {Details}" : $"{msg}.";
            }
        }

        public EventServiceCreationError(string? serviceName, string? details = null)
        {
            ServiceName = serviceName;
            Details = details;
        }
    }
}
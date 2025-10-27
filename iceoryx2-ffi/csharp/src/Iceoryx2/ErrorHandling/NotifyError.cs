namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during notify operation.
    /// </summary>
    public class NotifyError : Iox2Error
    {
        public EventId? EventId { get; }
        public override Iox2ErrorKind Kind => Iox2ErrorKind.NotifyFailed;
        public override string? Details { get; }
        public override string Message
        {
            get
            {
                var msg = EventId.HasValue
                    ? $"Failed to notify event {EventId.Value}"
                    : "Failed to notify event";
                return Details != null ? $"{msg}. Details: {Details}" : $"{msg}.";
            }
        }

        public NotifyError(EventId? eventId = null, string? details = null)
        {
            EventId = eventId;
            Details = details;
        }
    }
}
namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during subscriber creation.
    /// </summary>
    public class SubscriberCreationError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.SubscriberCreationFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to create subscriber. Details: {Details}"
            : "Failed to create subscriber.";

        public SubscriberCreationError(string? details = null)
        {
            Details = details;
        }
    }
}
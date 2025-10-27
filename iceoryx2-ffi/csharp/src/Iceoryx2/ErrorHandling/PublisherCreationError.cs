namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during publisher creation.
    /// </summary>
    public class PublisherCreationError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.PublisherCreationFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to create publisher. Details: {Details}"
            : "Failed to create publisher.";

        public PublisherCreationError(string? details = null)
        {
            Details = details;
        }
    }
}
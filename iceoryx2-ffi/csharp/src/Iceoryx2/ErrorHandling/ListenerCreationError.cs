namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during listener creation.
    /// </summary>
    public class ListenerCreationError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.ListenerCreationFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to create listener. Details: {Details}"
            : "Failed to create listener.";

        public ListenerCreationError(string? details = null)
        {
            Details = details;
        }
    }
}
namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during notifier creation.
    /// </summary>
    public class NotifierCreationError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.NotifierCreationFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to create notifier. Details: {Details}"
            : "Failed to create notifier.";

        public NotifierCreationError(string? details = null)
        {
            Details = details;
        }
    }
}
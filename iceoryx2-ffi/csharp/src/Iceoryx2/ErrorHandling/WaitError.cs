namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during wait operation.
    /// </summary>
    public class WaitError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.WaitFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to wait for event. Details: {Details}"
            : "Failed to wait for event.";

        public WaitError(string? details = null)
        {
            Details = details;
        }
    }
}
namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during receive operation.
    /// </summary>
    public class ReceiveError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.ReceiveFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to receive. Details: {Details}"
            : "Failed to receive.";

        public ReceiveError(string? details = null)
        {
            Details = details;
        }
    }
}
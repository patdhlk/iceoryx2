namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred when receiving a response.
    /// </summary>
    public class ResponseReceiveError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.ResponseReceiveFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to receive response. Details: {Details}"
            : "Failed to receive response.";

        public ResponseReceiveError(string? details = null)
        {
            Details = details;
        }
    }
}
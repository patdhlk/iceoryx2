namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred when sending a response.
    /// </summary>
    public class ResponseSendError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.ResponseSendFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to send response. Details: {Details}"
            : "Failed to send response.";

        public ResponseSendError(string? details = null)
        {
            Details = details;
        }
    }
}
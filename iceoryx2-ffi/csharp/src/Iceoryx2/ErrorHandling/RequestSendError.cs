namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred when sending a request.
    /// </summary>
    public class RequestSendError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.RequestSendFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to send request. Details: {Details}"
            : "Failed to send request.";

        public RequestSendError(string? details = null)
        {
            Details = details;
        }
    }
}
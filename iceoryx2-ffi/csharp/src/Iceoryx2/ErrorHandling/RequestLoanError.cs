namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred when loaning a request.
    /// </summary>
    public class RequestLoanError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.RequestLoanFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to loan request. Details: {Details}"
            : "Failed to loan request.";

        public RequestLoanError(string? details = null)
        {
            Details = details;
        }
    }
}
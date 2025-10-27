namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred when loaning a response.
    /// </summary>
    public class ResponseLoanError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.ResponseLoanFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to loan response. Details: {Details}"
            : "Failed to loan response.";

        public ResponseLoanError(string? details = null)
        {
            Details = details;
        }
    }
}
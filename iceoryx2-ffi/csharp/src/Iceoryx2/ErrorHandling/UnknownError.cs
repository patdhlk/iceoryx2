namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an unknown or unclassified error.
    /// </summary>
    public class UnknownError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.Unknown;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"An unknown error occurred. Details: {Details}"
            : "An unknown error occurred.";

        public UnknownError(string? details = null)
        {
            Details = details;
        }
    }
}
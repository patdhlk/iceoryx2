namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during WaitSet creation.
    /// </summary>
    public class WaitSetCreationError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.WaitSetCreationFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to create WaitSet. Details: {Details}"
            : "Failed to create WaitSet.";

        public WaitSetCreationError(string? details = null)
        {
            Details = details;
        }
    }
}
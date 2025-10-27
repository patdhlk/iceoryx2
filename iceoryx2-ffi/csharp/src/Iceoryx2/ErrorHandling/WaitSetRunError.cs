namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during WaitSet run operation.
    /// </summary>
    public class WaitSetRunError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.WaitSetRunFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to run WaitSet. Details: {Details}"
            : "Failed to run WaitSet.";

        public WaitSetRunError(string? details = null)
        {
            Details = details;
        }
    }
}
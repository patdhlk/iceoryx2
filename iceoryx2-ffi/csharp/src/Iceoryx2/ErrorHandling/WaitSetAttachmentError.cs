namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during WaitSet attachment.
    /// </summary>
    public class WaitSetAttachmentError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.WaitSetAttachmentFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to attach to WaitSet. Details: {Details}"
            : "Failed to attach to WaitSet.";

        public WaitSetAttachmentError(string? details = null)
        {
            Details = details;
        }
    }
}
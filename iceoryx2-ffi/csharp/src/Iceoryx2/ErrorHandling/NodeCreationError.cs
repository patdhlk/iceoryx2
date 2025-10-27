namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during node creation.
    /// </summary>
    public class NodeCreationError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.NodeCreationFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to create node. Details: {Details}"
            : "Failed to create node.";

        public NodeCreationError(string? details = null)
        {
            Details = details;
        }
    }
}
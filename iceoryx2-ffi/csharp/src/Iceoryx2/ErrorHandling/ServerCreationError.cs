namespace Iceoryx2.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during server creation.
    /// </summary>
    public class ServerCreationError : Iox2Error
    {
        public override Iox2ErrorKind Kind => Iox2ErrorKind.ServerCreationFailed;
        public override string? Details { get; }
        public override string Message => Details != null
            ? $"Failed to create server. Details: {Details}"
            : "Failed to create server.";

        public ServerCreationError(string? details = null)
        {
            Details = details;
        }
    }
}
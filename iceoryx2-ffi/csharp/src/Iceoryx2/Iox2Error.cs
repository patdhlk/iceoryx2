namespace Iceoryx2;

/// <summary>
/// Common error types for iceoryx2 operations.
/// </summary>
public enum Iox2Error
{
    NodeCreationFailed,
    ServiceCreationFailed,
    PublisherCreationFailed,
    SubscriberCreationFailed,
    SampleLoanFailed,
    SendFailed,
    ReceiveFailed,
    InvalidHandle,
    Unknown
}
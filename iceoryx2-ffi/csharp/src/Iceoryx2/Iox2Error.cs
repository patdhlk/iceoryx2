namespace Iceoryx2;

/// <summary>
/// Indicates that a loan for a sample could not be obtained during the operation.
/// This error may occur when the underlying memory management system for samples fails
/// to grant the requested loan, such as due to insufficient memory resources.
/// </summary>
public enum Iox2Error
{
    /// <summary>
    /// Represents an unknown or undefined error condition. This value may be used
    /// when the specific error type cannot be determined or mapped to existing
    /// defined errors.
    /// </summary>
    NodeCreationFailed,

    /// <summary>
    /// Indicates that the creation of a service failed. This error is typically encountered
    /// when the necessary resources or configurations required to establish the service
    /// cannot be fulfilled or initialized during the service creation process.
    /// </summary>
    ServiceCreationFailed,

    /// <summary>
    /// Indicates a failure during the creation of a publisher.
    /// </summary>
    PublisherCreationFailed,

    /// <summary>
    /// Represents an error that occurs when the creation of a subscriber fails.
    /// This error may arise due to issues such as resource constraints, invalid
    /// configurations, or system-level failures during the subscriber creation process.
    /// </summary>
    SubscriberCreationFailed,

    /// <summary>
    /// Indicates that a loan for a sample could not be obtained during the operation.
    /// This error might occur when the system is unable to allocate the required resources
    /// to fulfill the sample loan request, often due to insufficient memory availability
    /// or conflicting resource constraints.
    /// </summary>
    SampleLoanFailed,

    /// <summary>
    /// Represents an error condition where the send operation failed.
    /// This error may occur if the data cannot be transmitted successfully due to
    /// issues like communication breakdown, insufficient resources, or invalid state.
    /// </summary>
    SendFailed,
    
    /// <summary>
    /// Represents an error condition where receiving data has failed.
    /// This may occur if the subscriber is unable to successfully retrieve
    /// the expected data, which could be due to internal errors or data unavailability.
    /// </summary>
    ReceiveFailed,

    /// <summary>
    /// Indicates that an operation failed due to an invalid handle being used.
    /// This error typically occurs when a handle provided to the system is
    /// unrecognized, uninitialized, or no longer valid.
    /// </summary>
    InvalidHandle,

    /// <summary>
    /// Represents an unspecified or unclassified error. This value may be used
    /// as a placeholder when the exact nature of the error is unknown or does not
    /// match predefined error cases within the Iox2Error enumeration.
    /// </summary>
    Unknown
}
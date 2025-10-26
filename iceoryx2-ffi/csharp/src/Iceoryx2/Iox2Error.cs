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
    /// Indicates a failure during the creation of a notifier.
    /// This error may occur when the system cannot allocate the required resources
    /// or establish the notifier for event-based communication.
    /// </summary>
    NotifierCreationFailed,

    /// <summary>
    /// Indicates a failure during the creation of a listener.
    /// This error may occur when the system cannot allocate the required resources
    /// or establish the listener for event-based communication.
    /// </summary>
    ListenerCreationFailed,

    /// <summary>
    /// Represents an error condition where the notify operation failed.
    /// This error may occur if the event notification cannot be sent to listeners
    /// due to issues like communication breakdown or invalid event ID.
    /// </summary>
    NotifyFailed,

    /// <summary>
    /// Represents an error condition where waiting for an event failed.
    /// This error may occur if the listener encounters an issue while waiting
    /// for event notifications, such as timeout or internal failure.
    /// </summary>
    WaitFailed,

    /// <summary>
    /// Indicates a failure during the creation of an event service.
    /// This error is encountered when the necessary resources or configurations
    /// required to establish the event service cannot be fulfilled.
    /// </summary>
    EventServiceCreationFailed,

    /// <summary>
    /// Indicates a failure during the creation of a request-response service.
    /// This error is encountered when the necessary resources or configurations
    /// required to establish the request-response service cannot be fulfilled.
    /// </summary>
    RequestResponseServiceCreationFailed,

    /// <summary>
    /// Indicates a failure during the creation of a client.
    /// This error may occur when the system cannot allocate the required resources
    /// or establish the client for request-response communication.
    /// </summary>
    ClientCreationFailed,

    /// <summary>
    /// Indicates a failure during the creation of a server.
    /// This error may occur when the system cannot allocate the required resources
    /// or establish the server for request-response communication.
    /// </summary>
    ServerCreationFailed,

    /// <summary>
    /// Represents an error condition where loaning a request failed.
    /// This error may occur if the client cannot allocate memory for the request
    /// due to resource constraints or invalid state.
    /// </summary>
    RequestLoanFailed,

    /// <summary>
    /// Represents an error condition where sending a request failed.
    /// This error may occur if the request cannot be transmitted to the server
    /// due to communication breakdown or resource issues.
    /// </summary>
    RequestSendFailed,

    /// <summary>
    /// Represents an error condition where loaning a response failed.
    /// This error may occur if the server cannot allocate memory for the response
    /// due to resource constraints or invalid state.
    /// </summary>
    ResponseLoanFailed,

    /// <summary>
    /// Represents an error condition where sending a response failed.
    /// This error may occur if the response cannot be transmitted back to the client
    /// due to communication breakdown or resource issues.
    /// </summary>
    ResponseSendFailed,

    /// <summary>
    /// Represents an error condition where receiving a response failed.
    /// This error may occur if the client cannot successfully retrieve the response
    /// from the server due to timeout or communication issues.
    /// </summary>
    ResponseReceiveFailed,

    /// <summary>
    /// Indicates that an operation failed due to an invalid handle being used.
    /// This error typically occurs when a handle provided to the system is
    /// unrecognized, uninitialized, or no longer valid.
    /// </summary>
    InvalidHandle,

    /// <summary>
    /// Indicates a failure during the creation of a WaitSet.
    /// This error may occur when the system cannot allocate the required resources
    /// or establish the WaitSet for event multiplexing.
    /// </summary>
    WaitSetCreationFailed,

    /// <summary>
    /// Indicates a failure when attaching to a WaitSet.
    /// This error may occur due to insufficient capacity, the object already being attached,
    /// or internal system errors.
    /// </summary>
    WaitSetAttachmentFailed,

    /// <summary>
    /// Indicates a failure during WaitSet wait and process operation.
    /// This error may occur due to insufficient permissions, no attachments,
    /// or internal system errors.
    /// </summary>
    WaitSetRunFailed,

    /// <summary>
    /// Represents an unspecified or unclassified error. This value may be used
    /// as a placeholder when the exact nature of the error is unknown or does not
    /// match predefined error cases within the Iox2Error enumeration.
    /// </summary>
    Unknown
}
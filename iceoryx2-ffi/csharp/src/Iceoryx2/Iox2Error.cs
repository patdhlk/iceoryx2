using Iceoryx2.ErrorHandling;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Iceoryx2;

/// <summary>
/// Base class for all iceoryx2 errors. Provides rich, contextual error information
/// that enables better diagnostics and troubleshooting.
/// </summary>
public abstract class Iox2Error
{
    /// <summary>
    /// Gets the error message describing what went wrong.
    /// </summary>
    public abstract string Message { get; }

    /// <summary>
    /// Gets the error kind for pattern matching and backward compatibility.
    /// </summary>
    public abstract Iox2ErrorKind Kind { get; }

    /// <summary>
    /// Gets additional details about the error, if available.
    /// </summary>
    public virtual string? Details { get; }

    /// <summary>
    /// Returns a string representation of the error.
    /// </summary>
    public override string ToString() => Message;

    /// <summary>
    /// Creates an Iox2Error from an error kind with optional details.
    /// </summary>
    public static Iox2Error FromKind(Iox2ErrorKind kind, string? details = null)
    {
        return kind switch
        {
            Iox2ErrorKind.NodeCreationFailed => new NodeCreationError(details),
            Iox2ErrorKind.ServiceCreationFailed => new ServiceCreationError(null, details),
            Iox2ErrorKind.PublisherCreationFailed => new PublisherCreationError(details),
            Iox2ErrorKind.SubscriberCreationFailed => new SubscriberCreationError(details),
            Iox2ErrorKind.SampleLoanFailed => new SampleLoanError(details),
            Iox2ErrorKind.SendFailed => new SendError(details),
            Iox2ErrorKind.ReceiveFailed => new ReceiveError(details),
            Iox2ErrorKind.NotifierCreationFailed => new NotifierCreationError(details),
            Iox2ErrorKind.ListenerCreationFailed => new ListenerCreationError(details),
            Iox2ErrorKind.NotifyFailed => new NotifyError(null, details),
            Iox2ErrorKind.WaitFailed => new WaitError(details),
            Iox2ErrorKind.EventServiceCreationFailed => new EventServiceCreationError(null, details),
            Iox2ErrorKind.RequestResponseServiceCreationFailed => new RequestResponseServiceCreationError(null, details),
            Iox2ErrorKind.ClientCreationFailed => new ClientCreationError(details),
            Iox2ErrorKind.ServerCreationFailed => new ServerCreationError(details),
            Iox2ErrorKind.RequestLoanFailed => new RequestLoanError(details),
            Iox2ErrorKind.RequestSendFailed => new RequestSendError(details),
            Iox2ErrorKind.ResponseLoanFailed => new ResponseLoanError(details),
            Iox2ErrorKind.ResponseSendFailed => new ResponseSendError(details),
            Iox2ErrorKind.ResponseReceiveFailed => new ResponseReceiveError(details),
            Iox2ErrorKind.InvalidHandle => new InvalidHandleError(details),
            Iox2ErrorKind.WaitSetCreationFailed => new WaitSetCreationError(details),
            Iox2ErrorKind.WaitSetAttachmentFailed => new WaitSetAttachmentError(details),
            Iox2ErrorKind.WaitSetRunFailed => new WaitSetRunError(details),
            Iox2ErrorKind.Unknown => new UnknownError(details),
            _ => new UnknownError(details)
        };
    }
    
    public static Iox2Error NodeCreationFailed => new NodeCreationError();
    public static Iox2Error ServiceCreationFailed => new ServiceCreationError(null);
    public static Iox2Error PublisherCreationFailed => new PublisherCreationError();
    public static Iox2Error SubscriberCreationFailed => new SubscriberCreationError();
    public static Iox2Error SampleLoanFailed => new SampleLoanError();
    public static Iox2Error SendFailed => new SendError();
    public static Iox2Error ReceiveFailed => new ReceiveError();
    public static Iox2Error NotifierCreationFailed => new NotifierCreationError();
    public static Iox2Error ListenerCreationFailed => new ListenerCreationError();
    public static Iox2Error NotifyFailed => new NotifyError();
    public static Iox2Error WaitFailed => new WaitError();
    public static Iox2Error EventServiceCreationFailed => new EventServiceCreationError(null);
    public static Iox2Error RequestResponseServiceCreationFailed => new RequestResponseServiceCreationError(null);
    public static Iox2Error ClientCreationFailed => new ClientCreationError();
    public static Iox2Error ServerCreationFailed => new ServerCreationError();
    public static Iox2Error RequestLoanFailed => new RequestLoanError();
    public static Iox2Error RequestSendFailed => new RequestSendError();
    public static Iox2Error ResponseLoanFailed => new ResponseLoanError();
    public static Iox2Error ResponseSendFailed => new ResponseSendError();
    public static Iox2Error ResponseReceiveFailed => new ResponseReceiveError();
    public static Iox2Error InvalidHandle => new InvalidHandleError();
    public static Iox2Error WaitSetCreationFailed => new WaitSetCreationError();
    public static Iox2Error WaitSetAttachmentFailed => new WaitSetAttachmentError();
    public static Iox2Error WaitSetRunFailed => new WaitSetRunError();
    public static Iox2Error Unknown => new UnknownError();
}
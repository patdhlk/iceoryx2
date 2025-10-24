# Request-Response (RPC) API Implementation

## Summary

The Request-Response API for iceoryx2 C# bindings has been successfully implemented and documented. This provides a complete client-server RPC pattern with type-safe async communication.

## Implementation Details

### 1. FFI Layer (`Iox2NativeMethods.cs`)
- Added 10+ native struct definitions for RPC types
- Added 30+ P/Invoke method declarations covering:
  - Service builder request-response API
  - Port factory request-response API
  - Client builder and operations (loan, send)
  - Server builder and operations (receive)
  - Request/Response payload access
  - Pending response with try/timed/blocking receive

### 2. SafeHandle Classes (Resource Management)
- `SafeRequestResponseServiceHandle.cs` - Port factory lifecycle
- `SafeClientHandle.cs` - Client resource management
- `SafeServerHandle.cs` - Server resource management
- `SafePendingResponseHandle.cs` - Pending response lifecycle

### 3. High-Level Wrappers
- **RequestResponseServiceBuilder.cs** - Fluent builder API for opening/creating services
  - `Open(serviceName)` - Open existing service
  - `Create(serviceName)` - Create new service
  - Handles type name and alignment calculation automatically
  
- **RequestResponseService.cs** - Port factory for creating clients and servers
  - `CreateClient()` - Create RPC client
  - `CreateServer()` - Create RPC server
  
- **Client.cs** - Send requests and receive responses
  - `Loan()` - Zero-copy request allocation
  - `SendCopy(TRequest)` - Convenience method for small requests
  
- **Server.cs** - Receive requests and send responses
  - `Receive()` - Non-blocking request reception
  
- **Request.cs** - Received request from client
  - `Payload` - Read request data
  - `LoanResponse()` - Zero-copy response allocation
  - `SendCopyResponse(TResponse)` - Convenience method for small responses
  
- **RequestMut.cs** - Mutable request to send
  - `Payload` - Read/write request data
  - `Send()` - Send request and get PendingResponse handle
  
- **Response.cs** - Received response from server
  - `Payload` - Read response data
  
- **ResponseMut.cs** - Mutable response to send
  - `Payload` - Read/write response data
  - `Send()` - Send response back to client
  
- **PendingResponse.cs** - Async response handle
  - `TryReceive()` - Non-blocking receive
  - `TimedReceive(TimeSpan)` - Receive with timeout
  - `BlockingReceive()` - Block until response arrives

### 4. Error Handling
Added 8 new error codes to `Iox2Error.cs`:
- `RequestResponseServiceCreationFailed`
- `ClientCreationFailed`
- `ServerCreationFailed`
- `RequestLoanFailed`
- `RequestSendFailed`
- `ResponseLoanFailed`
- `ResponseSendFailed`
- `ResponseReceiveFailed`

### 5. Integration
- Added `RequestResponse<TRequest, TResponse>()` method to `ServiceBuilder.cs`
- Follows same fluent API pattern as PublishSubscribe and Event APIs

### 6. Example Application
Created `examples/RequestResponse/` demonstrating:
- Client mode: Sends 10 AddRequest messages, waits for responses
- Server mode: Maintains running sum, processes requests continuously
- Proper resource management with `using` statements
- Result<T, E> error handling pattern
- Timeout handling with `TimedReceive()`

### 7. Documentation
Updated `README.md` with:
- Request-Response marked as complete in status section
- Added to supported communication patterns
- Complete usage example with client and server code
- Added to examples section
- Updated roadmap to show RPC as completed

## Build Status

✅ **All builds successful**:
- Iceoryx2.dll compiled with 0 errors
- RequestResponse example compiled successfully
- All existing examples still build correctly

## Usage Example

```csharp
// Define request/response types
[StructLayout(LayoutKind.Sequential)]
public struct AddRequest { public int Value; }

[StructLayout(LayoutKind.Sequential)]
public struct AddResponse { public int Sum; }

// Create service
using var node = NodeBuilder.New().Name("rpc_node").Create().Unwrap();
using var service = node.ServiceBuilder()
    .RequestResponse<AddRequest, AddResponse>()
    .Open("AddService")
    .Unwrap();

// Client: Send request and wait for response
using var client = service.CreateClient().Unwrap();
using var pending = client.SendCopy(new AddRequest { Value = 42 }).Unwrap();
var response = pending.TimedReceive(TimeSpan.FromSeconds(2)).Unwrap();
if (response != null)
{
    Console.WriteLine($"Sum: {response.Payload.Sum}");
}

// Server: Receive request and send response
using var server = service.CreateServer().Unwrap();
var request = server.Receive().Unwrap();
if (request != null)
{
    request.SendCopyResponse(new AddResponse { Sum = request.Payload.Value + 100 });
}
```

## Key Features

- **Type-safe generics**: `RequestResponse<TRequest, TResponse>` with unmanaged constraints
- **Zero-copy support**: `Loan()` methods for efficient memory sharing
- **Convenience methods**: `SendCopy()` and `SendCopyResponse()` for simple use cases
- **Async response handling**: Three receive modes (try, timed, blocking)
- **Automatic cleanup**: SafeHandle + IDisposable for resource management
- **Fluent API**: Builder pattern consistent with other iceoryx2 APIs
- **Result pattern**: IsOk/Unwrap for error handling

## Testing Status

- ✅ Builds successfully
- ✅ Example compiles without errors
- ⏳ Runtime testing pending (requires native iceoryx2 library)
- ⏳ Unit tests to be added

## Next Steps

1. Runtime testing with native library
2. Add unit tests for RPC API
3. Performance benchmarks
4. Consider adding advanced features:
   - Request cancellation
   - Streaming responses
   - Priority-based request handling

## Files Created/Modified

### New Files (13):
- `src/Iceoryx2/SafeHandles/SafeRequestResponseServiceHandle.cs`
- `src/Iceoryx2/SafeHandles/SafeClientHandle.cs`
- `src/Iceoryx2/SafeHandles/SafeServerHandle.cs`
- `src/Iceoryx2/SafeHandles/SafePendingResponseHandle.cs`
- `src/Iceoryx2/RequestResponse/RequestResponseServiceBuilder.cs`
- `src/Iceoryx2/RequestResponse/RequestResponseService.cs`
- `src/Iceoryx2/RequestResponse/Client.cs`
- `src/Iceoryx2/RequestResponse/Server.cs`
- `src/Iceoryx2/RequestResponse/Request.cs`
- `src/Iceoryx2/RequestResponse/RequestMut.cs`
- `src/Iceoryx2/RequestResponse/Response.cs`
- `src/Iceoryx2/RequestResponse/ResponseMut.cs`
- `src/Iceoryx2/RequestResponse/PendingResponse.cs`

### Modified Files (4):
- `src/Iceoryx2/Native/Iox2NativeMethods.cs` - Added RPC FFI layer
- `src/Iceoryx2/Types/Iox2Error.cs` - Added 8 RPC error codes
- `src/Iceoryx2/Core/ServiceBuilder.cs` - Added RequestResponse<T, R>() method
- `README.md` - Complete RPC documentation

### Example Files (2):
- `examples/RequestResponse/RequestResponse.csproj`
- `examples/RequestResponse/Program.cs`

I have implemented the requested connection management features and startup helper.

### Summary of Changes

1.  **BootNotification Logic & Session Reuse**:
    *   Updated `ChargePointInfo` to include `RegistrationStatus`.
    *   Updated `WebSocketServer` to check `StateCache` upon connection. If the charge point is already `Accepted`, the session is marked as `Verified` immediately (allowing session reuse without forced re-boot).
    *   Updated `WebSocketSession` to include a verification timer (60s default). If not verified within this time, the connection is actively closed.
    *   Updated `RequestHandler` to mark the session as `Verified` upon successful `BootNotification`.

2.  **WebSocket Server Startup**:
    *   Created `OcppServerHost` helper class to demonstrate how to configure and start the server using Kestrel and ASP.NET Core.
    *   Clarified that `OcppAdapter` is designed as a middleware library, but `OcppServerHost` provides a standalone entry point.

### Verification
You can verify the startup logic by using `OcppServerHost.RunAsync()` in your application entry point. The connection management logic is now embedded in the WebSocket handling flow.

I will now verify the compilation to ensure no breaking changes were introduced by the dependency injection updates.
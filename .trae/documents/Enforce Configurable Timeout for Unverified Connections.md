I will implement the requirement to enforce a timeout for unverified connections by making the timeout configurable and applying it to the session logic.

### Implementation Plan

1.  **Update Configuration**:
    *   Modify `OcppAdapterConfiguration.cs` to add a new property `UnverifiedSessionTimeout` (TimeSpan, default 60 seconds). This controls how long a new connection can remain open without passing BootNotification authentication.

2.  **Update `WebSocketSession`**:
    *   Refactor the constructor to accept a `TimeSpan verificationTimeout` parameter.
    *   Remove the hardcoded `VerificationTimeoutSeconds` constant.
    *   Initialize the internal verification timer using the passed timeout value.

3.  **Update `OcppWebSocketHandler`**:
    *   In `HandleWebSocketAsync`, when creating a new `WebSocketSession`, pass the `_configuration.UnverifiedSessionTimeout` value to the constructor.

### Verification Strategy
*   **Configuration Check**: Verify the new config property exists and defaults to 60s.
*   **Timeout Enforcement**: A new connection that does not send BootNotification (or sends one that gets rejected) should be automatically closed by the server after the configured timeout.
*   **Valid Session**: A connection that successfully completes BootNotification within the timeout should NOT be closed.

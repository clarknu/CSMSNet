Based on the analysis of your requirements and the current codebase, I have identified several gaps and propose the following implementation plan to ensure strict compliance with your business logic.

### Gap Analysis

1.  **Session Retention**: Currently, the system immediately removes the session and state cache upon network disconnection. It fails to retain them for the required period to allow for reconnection reuse.
2.  **Reconnection Logic**: There is no logic to reuse an existing "disconnected" session upon reconnection.
3.  **BootNotification Enforcement**: The system does not strictly enforce that `BootNotification` must be the first message for a new or timed-out session.
4.  **Heartbeat Handling**: The current implementation waits for the external event subscriber to reply, whereas your requirement states it should be a notification only, and the connection layer should reply immediately.
5.  **Event Granularity**: The current `OnChargePointDisconnected` implies both network and session loss. We need to distinguish between "Network Disconnected" (but session kept) and "Session/Cache Destroyed" (after timeout).

### Implementation Plan

#### 1. Refactor `WebSocketSession`
*   **Add State**: Introduce a state to track `IsConnected` and `LastDisconnectTime`.
*   **Support Reconnection**: Add a method `ReplaceSocket(WebSocket newSocket)` to allow an existing session to be revived with a new network connection.

#### 2. Upgrade `ConnectionManager`
*   **Session Retention**: Modify `RemoveSession` to **not** remove the session immediately upon network disconnect. Instead, mark it as `Disconnected` and record the timestamp.
*   **Reconnection Handling**: Update `AddSession` to check for an existing disconnected session:
    *   **Within Timeout**: Reuse the existing session (call `ReplaceSocket`), update the session ID (if needed), and fire a "Network Reconnected" event.
    *   **Exceeded Timeout**: Clean up the old session (fire "Session Destroyed") and create a new one.
*   **Timeout Cleanup**: Update `CleanupInactiveSessions` to specifically target disconnected sessions that have exceeded the configuration `SessionRetentionTimeout` (distinct from idle timeout).

#### 3. Enhance `RequestHandler`
*   **Enforce BootNotification**: In `HandleRequestAsync`, check if the session is `Verified`. If not, and the request is NOT `BootNotification`, reject it with a specific error (requiring registration).
*   **Non-blocking Heartbeat**: Modify `HandleHeartbeatAsync` to:
    1.  Update local state (LastCommunication).
    2.  Fire `OnHeartbeat` event asynchronously (do not await user response).
    3.  Immediately return the `HeartbeatResponse` with server time.

#### 4. Event System Refinement
*   **Network vs. Session Events**:
    *   `OnChargePointDisconnected`: Trigger when the network connection drops.
    *   Add/Use `OnSessionCleanedUp` (or similar): Trigger when the session/cache is actually destroyed after the timeout.
*   **BootNotification Event**: Ensure the session is only marked "Verified" (and thus valid for business) after the `BootNotification` event returns `Accepted`.

### Verification Strategy
*   **Simulate Disconnect**: Connect a device, disconnect it, and verify the session remains in memory.
*   **Simulate Reconnect**: Reconnect within the timeout and verify the previous state (e.g., transaction info) is still accessible.
*   **Simulate Timeout**: Disconnect, wait for timeout, and verify the session is removed.
*   **Heartbeat Test**: Send a heartbeat and ensure an immediate response without blocking, while still receiving the event notification.

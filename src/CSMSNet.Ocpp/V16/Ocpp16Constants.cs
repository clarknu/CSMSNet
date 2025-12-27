namespace CSMSNet.Ocpp.V16;

/// <summary>
/// OCPP 1.6常量
/// </summary>
public static class Ocpp16Constants
{
    /// <summary>
    /// 协议版本
    /// </summary>
    public const string Version = "1.6";

    /// <summary>
    /// WebSocket子协议
    /// </summary>
    public const string SubProtocol = "ocpp1.6";

    /// <summary>
    /// 消息类型索引
    /// </summary>
    public static class MessageTypeIndex
    {
        public const int Call = 0;
        public const int CallResult = 0;
        public const int CallError = 0;
    }

    /// <summary>
    /// Call消息索引
    /// </summary>
    public static class CallIndex
    {
        public const int MessageTypeId = 0;
        public const int MessageId = 1;
        public const int Action = 2;
        public const int Payload = 3;
        public const int Length = 4;
    }

    /// <summary>
    /// CallResult消息索引
    /// </summary>
    public static class CallResultIndex
    {
        public const int MessageTypeId = 0;
        public const int MessageId = 1;
        public const int Payload = 2;
        public const int Length = 3;
    }

    /// <summary>
    /// CallError消息索引
    /// </summary>
    public static class CallErrorIndex
    {
        public const int MessageTypeId = 0;
        public const int MessageId = 1;
        public const int ErrorCode = 2;
        public const int ErrorDescription = 3;
        public const int ErrorDetails = 4;
        public const int Length = 5;
    }
}

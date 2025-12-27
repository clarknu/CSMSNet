namespace CSMSNet.OcppAdapter.Exceptions;

/// <summary>
/// OCPP异常基类
/// </summary>
public class OcppException : Exception
{
    public OcppException()
    {
    }

    public OcppException(string message) : base(message)
    {
    }

    public OcppException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// OCPP协议异常
/// </summary>
public class OcppProtocolException : OcppException
{
    public string? ErrorCode { get; set; }
    public string? ErrorDescription { get; set; }
    public object? ErrorDetails { get; set; }

    public OcppProtocolException()
    {
    }

    public OcppProtocolException(string message) : base(message)
    {
    }

    public OcppProtocolException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public OcppProtocolException(string errorCode, string errorDescription) 
        : base($"{errorCode}: {errorDescription}")
    {
        ErrorCode = errorCode;
        ErrorDescription = errorDescription;
    }
}

/// <summary>
/// 连接异常
/// </summary>
public class OcppConnectionException : OcppException
{
    public string? ChargePointId { get; set; }

    public OcppConnectionException()
    {
    }

    public OcppConnectionException(string message) : base(message)
    {
    }

    public OcppConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public OcppConnectionException(string chargePointId, string message) : base(message)
    {
        ChargePointId = chargePointId;
    }
}

/// <summary>
/// 超时异常
/// </summary>
public class OcppTimeoutException : OcppException
{
    public TimeSpan Timeout { get; set; }

    public OcppTimeoutException()
    {
    }

    public OcppTimeoutException(string message) : base(message)
    {
    }

    public OcppTimeoutException(string message, TimeSpan timeout) : base(message)
    {
        Timeout = timeout;
    }

    public OcppTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

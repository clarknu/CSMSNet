namespace CSMSNet.OcppAdapter.Models.Enums;

/// <summary>
/// 重复连接处理策略
/// </summary>
public enum DuplicateConnectionStrategy
{
    /// <summary>
    /// 关闭旧连接,接受新连接(默认)
    /// </summary>
    Replace,

    /// <summary>
    /// 拒绝新连接,保持旧连接
    /// </summary>
    Reject,

    /// <summary>
    /// 允许重复连接(不推荐)
    /// </summary>
    Duplicate
}

using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// ClearCache请求
/// </summary>
public class ClearCacheRequest : OcppRequest
{
    public override string Action => "ClearCache";
}

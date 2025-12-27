namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// GetLocalListVersion请求
/// </summary>
public class GetLocalListVersionRequest : OcppRequest
{
    public override string Action => "GetLocalListVersion";
}

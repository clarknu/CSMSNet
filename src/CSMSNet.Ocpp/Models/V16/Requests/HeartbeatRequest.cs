using System.Text.Json.Serialization;
using CSMSNet.OcppAdapter.Models;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// Heartbeat请求
/// </summary>
public class HeartbeatRequest : OcppRequest
{
    public override string Action => "Heartbeat";
}

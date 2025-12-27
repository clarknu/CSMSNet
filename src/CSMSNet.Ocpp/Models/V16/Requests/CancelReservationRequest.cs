using System.Text.Json.Serialization;

namespace CSMSNet.OcppAdapter.Models.V16.Requests;

/// <summary>
/// CancelReservation请求
/// </summary>
public class CancelReservationRequest : OcppRequest
{
    public override string Action => "CancelReservation";

    /// <summary>
    /// 预约ID
    /// </summary>
    [JsonPropertyName("reservationId")]
    public int ReservationId { get; set; }
}

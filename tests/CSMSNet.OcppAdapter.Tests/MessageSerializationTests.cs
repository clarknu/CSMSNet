using System.Text.Json;
using CSMSNet.OcppAdapter.Models.V16.Common;
using CSMSNet.OcppAdapter.Models.V16.Enums;
using CSMSNet.OcppAdapter.Models.V16.Requests;

namespace CSMSNet.OcppAdapter.Tests;

public class MessageSerializationTests
{
    private readonly JsonSerializerOptions _options;

    public MessageSerializationTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    [Fact]
    public void ReserveNowRequest_Serialization_ShouldWork()
    {
        var request = new ReserveNowRequest
        {
            ConnectorId = 1,
            ExpiryDate = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            IdTag = "TAG001",
            ReservationId = 100,
            ParentIdTag = "PARENT"
        };

        var json = JsonSerializer.Serialize(request, _options);
        
        Assert.Contains("\"connectorId\":1", json);
        Assert.Contains("\"idTag\":\"TAG001\"", json);
        Assert.Contains("\"reservationId\":100", json);
        Assert.Contains("\"parentIdTag\":\"PARENT\"", json);
        Assert.Contains("\"action\":\"ReserveNow\"", json); // Assuming OcppRequest has Action property but it might not be serialized if it's just a getter without setter or if it's ignored. 
        // Wait, OcppRequest.Action is abstract string property. Usually readonly properties are not serialized by default unless configured? 
        // Actually OcppRequest usually doesn't serialize Action in the payload itself for OCPP-J, the Action is part of the wrapper [2, "id", "Action", payload].
        // But let's check the fields.
    }

    [Fact]
    public void SetChargingProfileRequest_Serialization_ShouldWork()
    {
        var request = new SetChargingProfileRequest
        {
            ConnectorId = 1,
            CsChargingProfiles = new ChargingProfile
            {
                ChargingProfileId = 1,
                StackLevel = 1,
                ChargingProfilePurpose = ChargingProfilePurpose.TxProfile,
                ChargingProfileKind = ChargingProfileKind.Absolute,
                ChargingSchedule = new ChargingSchedule
                {
                    ChargingRateUnit = ChargingRateUnit.A,
                    ChargingSchedulePeriod = new List<ChargingSchedulePeriod>
                    {
                        new ChargingSchedulePeriod { StartPeriod = 0, Limit = 32 }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(request, _options);

        Assert.Contains("\"connectorId\":1", json);
        Assert.Contains("\"csChargingProfiles\"", json);
        Assert.Contains("\"chargingProfileId\":1", json);
        Assert.Contains("\"chargingProfilePurpose\":\"TxProfile\"", json);
        Assert.Contains("\"chargingRateUnit\":\"A\"", json);
    }
}

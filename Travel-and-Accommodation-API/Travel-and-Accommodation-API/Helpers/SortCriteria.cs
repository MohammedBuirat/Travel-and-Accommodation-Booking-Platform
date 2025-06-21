using System.Text.Json.Serialization;

namespace Travel_and_Accommodation_API.Helpers
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SortCriteria
    {
        Price,
        Rating,
        DistanceFromCity
    }
}

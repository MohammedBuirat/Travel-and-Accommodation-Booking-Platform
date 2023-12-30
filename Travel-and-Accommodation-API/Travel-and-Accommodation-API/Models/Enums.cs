using System.Text.Json.Serialization;

namespace Travel_and_Accommodation_API.Models
{
    public class Enums
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum RoomType
        {
            Luxury = 0,
            Budget = 1,
            Boutique = 2
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Flags]
        public enum Amenities : long
        {
            AirConditioning = 1L << 0,
            Heating = 1L << 1,
            TV = 1L << 2,
            InRoomSafe = 1L << 3,
            MiniBar = 1L << 4,
            Refrigerator = 1L << 5,
            OnSiteRestaurant = 1L << 6,
            RoomService = 1L << 7,
            BreakfastBuffet = 1L << 8,
            Bar = 1L << 9,
            Gym = 1L << 10,
            SwimmingPool = 1L << 11,
            Spa = 1L << 12,
            Sauna = 1L << 13,
            GameRoom = 1L << 14,
            WiFi = 1L << 15,
            FrontDesk24Hour = 1L << 16,
            LuggageStorage = 1L << 17,
            ValetParking = 1L << 18,
            SmartRoom = 1L << 19,
            ShuttleService = 1L << 20,
            DesibilitiesFreindly = 1L << 21,
            Elvator = 1L << 22,
            PetFreindly = 1L << 23,
            LaundryFacilities = 1L << 24
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum PaymentMethod
        {
            Cash,
            Card,
            PayPal
        }
    }
}

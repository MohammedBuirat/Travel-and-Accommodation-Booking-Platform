using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Helpers
{
    public static class AmenitiesExtensions
    {
        public static List<string> ConvertAmenitiesToStrings(this Amenities amenities)
        {
            return Enum.GetValues(typeof(Amenities))
                       .Cast<Amenities>()
                       .Where(a => amenities.HasFlag(a) && amenities != 0)
                       .Select(a => a.ToString())
                       .ToList();
        }

        public static Amenities ConvertStringsToAmenities(this IEnumerable<string> amenityStrings)
        {
            Amenities result = 0;
            if(amenityStrings == null)
            {
                return 0;
            }
            foreach (var amenityString in amenityStrings)
            {
                if (Enum.TryParse<Amenities>(amenityString, out var amenity))
                {
                    result |= amenity;
                }
            }
            return result;
        }
    }
}

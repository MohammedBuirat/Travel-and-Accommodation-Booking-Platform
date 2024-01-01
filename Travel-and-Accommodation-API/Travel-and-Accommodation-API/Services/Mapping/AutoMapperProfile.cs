using AutoMapper;
using Travel_and_Accommodation_API.Dto.Attraction;
using Travel_and_Accommodation_API.Dto.Booking;
using Travel_and_Accommodation_API.Dto.BookingRoom;
using Travel_and_Accommodation_API.Dto.CartBooking;
using Travel_and_Accommodation_API.Dto.CartBookingRoom;
using Travel_and_Accommodation_API.Dto.City;
using Travel_and_Accommodation_API.Dto.Country;
using Travel_and_Accommodation_API.Dto.Hotel;
using Travel_and_Accommodation_API.Dto.HotelImage;
using Travel_and_Accommodation_API.Dto.Payment;
using Travel_and_Accommodation_API.Dto.Review;
using Travel_and_Accommodation_API.Dto.Room;
using Travel_and_Accommodation_API.Dto.RoomDay;
using Travel_and_Accommodation_API.Dto.User;
using Travel_and_Accommodation_API.Dto.UserLastHotels;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.Services.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<UserToAdd, User>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => true)) // Assuming default value
            .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => DateTimeOffset.UtcNow));

            CreateMap<User, UserDto>().ReverseMap();

            //Room
            CreateMap<Room, RoomDto>()
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Amenities.ConvertAmenitiesToStrings()));

            CreateMap<RoomDto, Room>()
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Amenities.ConvertStringsToAmenities()));

            CreateMap<RoomToAdd, Room>()
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Amenities.ConvertStringsToAmenities()));

            // Booking mappings
            CreateMap<Booking, BookingDto>().ReverseMap();
            CreateMap<BookingToAdd, Booking>()
            .ForMember(dest => dest.BookingRooms, opt => opt.MapFrom(src => StringRoomsToBookingRoom(src.Rooms)));

            // City mappings
            CreateMap<City, CityDto>().ReverseMap();
            CreateMap<City, CityToAdd>().ReverseMap();

            // Hotel mappings
            CreateMap<Hotel, HotelDto>().ReverseMap();
            CreateMap<Hotel, HotelToAdd>().ReverseMap();

            // Payment mappings
            CreateMap<Payment, PaymentDto>().ReverseMap();
            CreateMap<Payment, PaymentToAdd>().ReverseMap();

            // Review mappings
            CreateMap<Review, ReviewDto>().ReverseMap();
            CreateMap<Review, ReviewToAdd>().ReverseMap();

            // HotelImage mappings
            CreateMap<HotelImage, HotelImageToAdd>().ReverseMap();

            // RoomDay mappings
            CreateMap<RoomDay, RoomDayDto>().ReverseMap();

            // BookingRoom mappings
            CreateMap<BookingRoom, BookingRoomDto>().ReverseMap();

            // CartBooking mappings
            CreateMap<CartBooking, CartBookingDto>().ReverseMap();
            CreateMap<CartBookingToAdd, CartBooking>()
                .ForMember(dest => dest.BookingRooms, opt => opt.MapFrom(src => StringRoomsToCartBookingRoom(src.Rooms))); ;

            // CartBookingRoom mappings
            CreateMap<CartBookingRoom, CartBookingRoomDto>().ReverseMap();

            //UserLastHotels mappings
            CreateMap<UserLastHotels, UserLastHotelsDto>().ReverseMap();

            //Attractions mappings
            CreateMap<Attraction, AttractionDto>().ReverseMap();
            CreateMap<Attraction, AttractionToAdd>().ReverseMap();

            //Country mappings
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<Country, CountryToAdd>().ReverseMap();

            //RoomWithPrice mapping
            CreateMap<RoomWithPrice, RoomWithPriceDto>()
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Amenities.ConvertAmenitiesToStrings()));

            //HotelWithPrice Mapping
            CreateMap<HotelWithPrice, HotelWithPriceDto>()
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Amenities.ConvertAmenitiesToStrings()));
        }

        private List<BookingRoom> StringRoomsToBookingRoom(List<Guid> rooms)
        {

            var bookingRooms = new List<BookingRoom>();
            if(rooms == null || rooms.Count() == 0)
            {
                return bookingRooms;
            }
            foreach (var room in rooms)
            {
                bookingRooms.Add(new BookingRoom { RoomId = room });
            }
            return bookingRooms;
        }

        private List<CartBookingRoom> StringRoomsToCartBookingRoom(List<Guid> rooms)
        {
            var bookingRooms = new List<CartBookingRoom>();
            if (rooms == null || rooms.Count() == 0)
            {
                return bookingRooms;
            }
            foreach (var room in rooms)
            {
                bookingRooms.Add(new CartBookingRoom { RoomId = room });
            }
            return bookingRooms;
        }
    }
}

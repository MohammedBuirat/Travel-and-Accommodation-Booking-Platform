using Travel_and_Accommodation_API.Dto.HotelImage;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IHotelImageService
    {
        public Task<IEnumerable<HotelImageDto>> GetAllAsync(int? pageSize, int? pageNumber);
        public Task AddAsync(HotelImageToAdd hotelImage);
        public Task<IEnumerable<HotelImageDto>> GetByHotelIdAsync(Guid hotelId);
        public Task<HotelImageDto> GetAsync(Guid hotelId, string imagePath);
        public Task DeleteAsync(Guid hotelId, string imagePath);
    }
}

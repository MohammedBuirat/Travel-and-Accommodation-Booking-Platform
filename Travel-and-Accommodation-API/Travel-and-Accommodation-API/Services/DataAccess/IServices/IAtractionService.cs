using Microsoft.AspNetCore.JsonPatch;
using Travel_and_Accommodation_API.Dto.Attraction;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.ImageService;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IAttractionService
    {
        public Task UpdateAsync(Guid id, AttractionToAdd entity);
        public Task<AttractionDto> AddAsync(AttractionToAdd attractionToAdd);
        public Task<IEnumerable<AttractionDto>> GetCityAttractionsAsync(Guid cityId, int? pageSize, int? pageNumber);
        public Task<IEnumerable<AttractionDto>> GetAllAsync(int? pageSize, int? pageNumber);
        public Task<AttractionDto> GetByIdAsync(Guid id);
        public Task DeleteAsync(Guid id);
        public Task PartialUpdateAsync(Guid id, JsonPatchDocument<Attraction> patchDocument);
    }
}

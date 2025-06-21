using Microsoft.AspNetCore.JsonPatch;
using Travel_and_Accommodation_API.Dto.City;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface ICityService
    {
        public Task<CityDto> GetByIdAsync(Guid id);
        public Task DeleteAsync(Guid id);
        public Task<IEnumerable<CityDto>> GetAllAsync(int? pageSize, int? pageNumber);
        public Task UpdateAsync(Guid id, CityToAdd entity);
        public Task PartialUpdateAsync(Guid id, JsonPatchDocument<City> patchDocument);
        public Task<CityDto> AddAsync(CityToAdd cityToAdd);
        public Task<IEnumerable<City>> MostVisitedCitiesAsync(int? pageSize, int? pageNumber);
        public Task<IEnumerable<CityDto>> SearchCitiesAsync(string name, int? pageSize, int? pageNumber);
        public Task<IEnumerable<CityDto>> AdminCitiesSearchAsync(
            string? name, int? pageSize, int? pageNumber, Guid? country, DateTimeOffset? creationDate);
    }
}

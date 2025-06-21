using Microsoft.AspNetCore.JsonPatch;
using Travel_and_Accommodation_API.Dto.Country;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface ICountryService
    {
        Task<CountryDto> GetByIdAsync(Guid id);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<CountryDto>> GetAllAsync(int? pageSize, int? pageNumber);
        Task UpdateAsync(Guid id, CountryToAdd entity);
        Task PartialUpdateAsync(Guid id, JsonPatchDocument<Country> patchDocument);
        Task<CountryDto> AddAsync(CountryToAdd countryToAdd);
    }
}

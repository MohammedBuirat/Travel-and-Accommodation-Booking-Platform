using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public interface IMostVisitedCitiesUnitOfWork
    {
        public Task<IEnumerable<City>> MostVisitedCitiesAsync(Paging paging);
    }
}

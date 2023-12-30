using System.Linq.Expressions;

namespace Travel_and_Accommodation_API.Helpers
{
    public class CustomExpression<T> where T : class
    {
        public Expression<Func<T, bool>> Filter { get; set; }
        public Expression<Func<T, bool>>? Sort { get; set; }
        public Paging Paging { get; set; }
        public bool Desc { get; set; } = false;
    }
}

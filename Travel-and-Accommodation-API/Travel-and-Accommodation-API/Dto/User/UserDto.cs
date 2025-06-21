namespace Travel_and_Accommodation_API.Dto.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int CountryId { get; set; }
        public DateTime BirthDate { get; set; }
    }
}

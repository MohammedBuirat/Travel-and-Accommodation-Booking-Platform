using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.DataAccess
{
    public class TravelAndAccommodationContext : IdentityDbContext
    {
        private readonly IConfiguration _configuration;
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingRoom> BookingRooms { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<HotelImage> HotelImages { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RoomDay> RoomDays { get; set; }
        public DbSet<CartBooking> CartBookings { get; set; }
        public DbSet<CartBookingRoom> CartBookingRooms { get; set; }
        public DbSet<UserLastHotels> UserLastHotels { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<HotelWithPrice> HotelsWithPrice { get; set; }

        public TravelAndAccommodationContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(_configuration["DataBase Connection"]);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RoomDay>().HasKey(rd => rd.Id);
            modelBuilder.Entity<BookingRoom>().HasKey(br => br.Id);
            modelBuilder.Entity<CartBookingRoom>().HasKey(cr => cr.Id);
            modelBuilder.Entity<UserLastHotels>().HasKey(ul => ul.Id);
            modelBuilder.Entity<HotelImage>().HasKey(hi => hi.Id);
            modelBuilder.Entity<Booking>().HasKey(b => b.Id);
            modelBuilder.Entity<City>().HasKey(b => b.Id);
            modelBuilder.Entity<Hotel>().HasKey(b => b.Id);
            modelBuilder.Entity<Payment>().HasKey(b => b.Id);
            modelBuilder.Entity<Review>().HasKey(b => b.Id);
            modelBuilder.Entity<Room>().HasKey(b => b.Id);
            modelBuilder.Entity<CartBooking>().HasKey(b => b.Id);
            modelBuilder.Entity<Attraction>().HasKey(b => b.Id);
            modelBuilder.Entity<Country>().HasKey(b => b.Id);
            SeedCountries(modelBuilder);
            SeedRoles(modelBuilder);
            SeedAdminUser(modelBuilder);
        }

        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData
                (
                    new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                    new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" }
                );
        }

        private void SeedCountries(ModelBuilder builder)
        {
            builder.Entity<Country>().HasData(new Country
            {
                Id = Guid.Parse("c34f9ac6-7985-44f8-b6c3-5be7b3e6b402"),
                Name = "Christmas Island",
                Language = "English",
                Currency = "Australian Dollar",
                CountryCode = "CX",
                TimeZone = "UTC+07:00",
                FlagImage = "cx_flag.png"
            });
        }

        private void SeedAdminUser(ModelBuilder builder)
        {
            var adminEmail = _configuration["Admin:Email"];
            var adminUserName = _configuration["Admin:UserName"];
            var adminPassword = _configuration["Admin:Password"];

            var hasher = new PasswordHasher<User>();
            builder.Entity<User>().HasData(new User
            {
                Id = "1",
                UserName = adminUserName,
                NormalizedUserName = adminUserName.ToUpper(),
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, adminPassword),
                FirstName = "Admin",
                LastName = "Admin",
                CountryId = Guid.Parse("c34f9ac6-7985-44f8-b6c3-5be7b3e6b402")
            });

            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = "1",
                UserId = "1"
            });
        }
    }
}
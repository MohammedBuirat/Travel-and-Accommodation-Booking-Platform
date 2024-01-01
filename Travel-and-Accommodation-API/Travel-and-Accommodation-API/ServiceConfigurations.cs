using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Travel_and_Accommodation_API.DataAccess;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.DataAccess.Repositories.RepositoryImplementation;
using Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks;
using Travel_and_Accommodation_API.DataAccess.UnitOfWork.UnitOFWork;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Security;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.DataAccess.Services;
using Travel_and_Accommodation_API.Services.EmailService;
using Travel_and_Accommodation_API.Services.ImageService;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API
{
    public static class ServiceConfigurations
    {
        public static void ConfigureRepositories(this IServiceCollection services)
        {
            services.AddScoped<IRepository<User>, Repository<User>>();
            services.AddScoped<IRepository<City>, Repository<City>>();
            services.AddScoped<IRepository<Hotel>, Repository<Hotel>>();
            services.AddScoped<IRepository<HotelImage>, Repository<HotelImage>>();
            services.AddScoped<IRepository<Room>, Repository<Room>>();
            services.AddScoped<IRoomDayRepository, RoomDayRepository>();
            services.AddScoped<IRepository<Booking>, Repository<Booking>>();
            services.AddScoped<IRepository<CartBooking>, Repository<CartBooking>>();
            services.AddScoped<IRepository<BookingRoom>, Repository<BookingRoom>>();
            services.AddScoped<IRepository<Payment>, Repository<Payment>>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IRepository<CartBookingRoom>, Repository<CartBookingRoom>>();
            services.AddScoped<IRepository<Attraction>, Repository<Attraction>>();
            services.AddScoped<IRepository<UserLastHotels>, Repository<UserLastHotels>>();
            services.AddScoped<IRepository<Country>, Repository<Country>>();
        }

        public static void ConfigureUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IBookingRoomUnitOfWork, BookingRoomUnitOfWork>();
            services.AddScoped<IBookingUnitOfWork, BookingUnitOfWork>();
            services.AddScoped<IGetTopDiscountedHotelsUnitOfWork, GetTopDiscountedHotelsUnitOfWork>();
            services.AddScoped<IMostVisitedCitiesUnitOfWork, MostVisitedCitiesUnitOfWork>();
            services.AddScoped<IPaymentUnitOfWork, PaymentUnitOfWork>();
            services.AddScoped<ISearchedFilteredHotelsUnitOfWork, SearchedFilteredHotelsUnitOfWork>();
            services.AddScoped<ISearchFilteredRoomsUnitOfWork, SearchFilteredRoomsUnitOfWork>();

        }
        public static void ConfigureDataAccessServices(this IServiceCollection services)
        {
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IHotelService, HotelService>();
            services.AddScoped<IHotelImageService, HotelImageService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IRoomDayService, RoomDayService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<ICartBookingService, CartBookingService>();
            services.AddScoped<IBookingRoomService, BookingRoomService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ICartBookingRoomService, CartBookingRoomService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IAttractionService, AttractionService>();
            services.AddScoped<IUserLastHotelsService, UserLastHotelsService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IUserService, UserService>();
        }

        public static void ConfigureValidations(this IServiceCollection services)
        {
            services.AddScoped<UserValidation>();
            services.AddScoped<CityValidation>();
            services.AddScoped<HotelValidation>();
            services.AddScoped<HotelImageValidation>();
            services.AddScoped<RoomValidation>();
            services.AddScoped<RoomDayValidation>();
            services.AddScoped<BookingValidation>();
            services.AddScoped<CartBookingValidation>();
            services.AddScoped<BookingRoomValidation>();
            services.AddScoped<PaymentValidation>();
            services.AddScoped<CartBookingRoomValidation>();
            services.AddScoped<ReviewValidation>();
            services.AddScoped<AttractionValidation>();
            services.AddScoped<CountryValidation>();
        }

        public static void ConfigureRateLimiting(this IServiceCollection services)
        {
            services.Configure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = true;
                options.StackBlockedRequests = false;
                options.HttpStatusCode = 429;
                options.RealIpHeader = "X-Real-Ip";
                options.ClientIdHeader = "X-ClientId";
                options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Period = "20s",
                        Limit = 5
                    }
                };
            });

            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            services.AddInMemoryRateLimiting();
        }

        public static void ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt =>
            {
                var key = Encoding.ASCII.GetBytes(configuration.GetSection("JwtConfig:Secret").Value);
                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // for dev
                    ValidateAudience = false, // for dev
                    RequireExpirationTime = true,
                    ValidateLifetime = true
                };
            });
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<TravelAndAccommodationContext>()
            .AddDefaultTokenProviders()
            .AddUserManager<UserManager<User>>();
        }

        public static void ConfigureDatabase(this IServiceCollection services)
        {
            services.AddScoped<TravelAndAccommodationContext>();
        }

        public static void ConfigureEmailAndImageServices(this IServiceCollection services)
        {
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IEmailService, EmailService>();
        }

        public static void ConfigureMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
        }

        public static void ConfigureSecurity(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<JwtManager>();
        }
        public static void ConfigureControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            })
            .AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters();
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public static void ConfigureAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Program).Assembly);
        }
    }
}

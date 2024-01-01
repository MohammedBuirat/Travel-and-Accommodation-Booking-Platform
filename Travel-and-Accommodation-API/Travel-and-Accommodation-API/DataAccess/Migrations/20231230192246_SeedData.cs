using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.VisualBasic.FileIO;
using Travel_and_Accommodation_API.Models;
using static Travel_and_Accommodation_API.Models.Enums;

#nullable disable

namespace Travel_and_Accommodation_API.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        public List<City> cities;
        public List<Room> rooms;
        public List<Hotel> hotels;

        protected override void Up(MigrationBuilder migrationBuilder)
        {

            List<Country> countries = ReadCsvFile();
            cities = ReadCsvFileCities();
            hotels = ReadCsvFileHotels();
            rooms = ReadCsvFileRooms();
            try
            {
                foreach (var country in countries)
                {
                    migrationBuilder.InsertData(
                    table: "Countries",
                    columns: new[] { "Id", "Name", "Currency", "Language", "CountryCode", "TimeZone", "FlagImage" },
                    values: new object[,]
                    {
                        {country.Id, country.Name, country.Currency, country.Language, country.CountryCode, country.TimeZone, country.FlagImage }
                    });
                }
                Console.WriteLine("Countries were added");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            try
            {
                foreach (var city in cities)
                {
                    migrationBuilder.InsertData(
                    table: "Cities",
                    columns: new[] { "Id", "Name", "PostOffice", "Image", "CountryId", "CountryName", "CreationDate", "ModificationDate" },
                    values: new object[,]
                    {
                        {city.Id, city.Name, city.PostOffice, city.Image, city.CountryId, city.CountryName, city.CreationDate, city.ModificationDate }
                    });
                }
                Console.WriteLine("Cities were added");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            try
            {
                foreach (var hotel in hotels)
                {
                    migrationBuilder.InsertData(
                    table: "Hotels",
                    columns: new[] { "Id", "Name", "Description", "CityId", "Owner", "CheckInTime", "CheckOutTime", "NumOfRatings", "SumOfRatings", "Address", "Latitude", "Longitude", "CreationDate", "ModificationDate", "DistanceFromCityCenter" },
                    values: new object[,]
                    {
                        {hotel.Id, hotel.Name, hotel.Description, hotel.CityId, hotel.Owner, hotel.CheckInTime, hotel.CheckOutTime, hotel.NumOfRatings, hotel.SumOfRatings, hotel.Address, hotel.Latitude, hotel.Longitude, hotel.CreationDate, null, hotel.DistanceFromCityCenter }
                    });
                }

                Console.WriteLine("Hotels were added");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                rooms = ReadCsvFileRooms();
                foreach (var room in rooms)
                {
                    migrationBuilder.InsertData(
                    table: "Rooms",
                    columns: new[] { "Id", "RoomNumber", "Description", "AdultsCapacity", "ChildrenCapacity", "Type", "CreationDate", "ModificationDate", "Amenities", "HotelId", "Image", "BasePrice" },
                    values: new object[,]
                    {
                        {room.Id, room.RoomNumber, room.Description, room.AdultsCapacity, room.ChildrenCapacity, (int)room.Type, room.CreationDate, room.ModificationDate,(int) room.Amenities, room.HotelId, room.Image, room.BasePrice}
                    });
                }
                Console.WriteLine("Rooms were added");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            try
            {
                foreach (var room in rooms)
                {
                    DateTime currentDate = DateTime.Today;
                    DateTime lastDate = currentDate.AddDays(30);

                    while (currentDate < lastDate)
                    {
                        RoomDay roomDay = new RoomDay();
                        roomDay.Id = Guid.NewGuid();
                        roomDay.Price = room.BasePrice;
                        roomDay.RoomId = room.Id;
                        roomDay.Available = true;
                        roomDay.Date = currentDate;

                        migrationBuilder.InsertData(
                            table: "RoomDays",
                            columns: new[] { "Id", "RoomId", "Price", "Date", "Available" },
                            values: new object[,]
                            {
                    {roomDay.Id, roomDay.RoomId, roomDay.Price, roomDay.Date, roomDay.Available}
                            });

                        currentDate = currentDate.AddDays(1);
                    }

                }
                Console.WriteLine("Room Days were added");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            List<Country> countries = ReadCsvFile();
            if (countries != null)
            {
                foreach (var country in countries)
                {
                    migrationBuilder.DeleteData(
                    table: "Countries",
                    keyColumn: "Id",
                    keyValue: country.Id);
                }
            }
            if (cities != null)
            {
                foreach (var city in cities)
                {
                    migrationBuilder.DeleteData(
                    table: "Cities",
                    keyColumn: "Id",
                        keyValue: city.Id);
                }
            }

            if (rooms != null)
            {
                foreach (var room in rooms)
                {
                    migrationBuilder.DeleteData(
                        table: "Rooms",
                        keyColumn: "Id",
                        keyValue: room.Id);
                }
            }
            if (hotels != null)
            {
                foreach (var hotel in hotels)
                {
                    migrationBuilder.DeleteData(
                        table: "Hotels",
                        keyColumn: "Id",
                        keyValue: hotel.Id);
                }
            }
        }

        List<Country> ReadCsvFile()
        {
            try
            {
                string csvFilePath = @"DataAccess\Data Seeding\countries.csv";

                List<Country> countries = new List<Country>();

                using (TextFieldParser parser = new TextFieldParser(csvFilePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    parser.ReadLine();

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        if (fields != null && fields.Length >= 7)
                        {
                            Country country = new Country
                            {
                                Id = Guid.Parse(fields[0]),
                                Name = fields[1],
                                Language = fields[2],
                                Currency = fields[3],
                                CountryCode = fields[4],
                                TimeZone = fields[5],
                                FlagImage = fields[6]
                            };

                            countries.Add(country);
                        }
                    }
                }
                return countries;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        List<City> ReadCsvFileCities()
        {
            try
            {
                string csvFilePath = @"DataAccess\Data Seeding\cities.csv";

                List<City> cities = new List<City>();

                using (TextFieldParser parser = new TextFieldParser(csvFilePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    parser.ReadLine(); // Skip header line

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        if (fields != null && fields.Length >= 8)
                        {
                            City city = new City
                            {
                                Id = Guid.Parse(fields[0]),
                                Name = fields[1],
                                PostOffice = fields[2],
                                Image = fields[3],
                                CountryId = Guid.Parse(fields[4]),
                                CountryName = fields[5],
                                CreationDate = DateTime.Parse(fields[6]),
                                ModificationDate = null
                            };

                            cities.Add(city);
                        }
                    }
                }
                return cities;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        List<Room> ReadCsvFileRooms()
        {
            try
            {
                string csvFilePath = @"DataAccess\Data Seeding\rooms.csv";

                List<Room> rooms = new List<Room>();

                using (TextFieldParser parser = new TextFieldParser(csvFilePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    parser.ReadLine();

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        if (fields != null && fields.Length >= 7)
                        {
                            Room room = new Room
                            {
                                Id = Guid.Parse(fields[0]),
                                RoomNumber = int.Parse(fields[1]),
                                Description = fields[2],
                                AdultsCapacity = int.Parse(fields[3]),
                                ChildrenCapacity = int.Parse(fields[4]),
                                Type = (RoomType)Enum.ToObject(typeof(RoomType), int.Parse(fields[5])),
                                Amenities = (Amenities)Enum.ToObject(typeof(Amenities), int.Parse(fields[6])),
                                HotelId = Guid.Parse(fields[7]),
                                Image = fields[8],
                                BasePrice = decimal.Parse(fields[9]),
                                CreationDate = DateTimeOffset.Now,
                                ModificationDate = null,
                            };

                            rooms.Add(room);
                        }
                    }
                }
                return rooms;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        List<Hotel> ReadCsvFileHotels()
        {
            try
            {
                string csvFilePath = @"DataAccess\Data Seeding\hotels.csv";

                List<Hotel> hotels = new List<Hotel>();

                using (TextFieldParser parser = new TextFieldParser(csvFilePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    parser.ReadLine(); // Skip header line

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        if (fields != null && fields.Length >= 15)
                        {
                            Hotel hotel = new Hotel
                            {
                                Id = Guid.Parse(fields[0]),
                                Name = fields[1],
                                Description = fields[2],
                                CityId = Guid.Parse(fields[3]),
                                Owner = fields[4],
                                CheckInTime = fields[5],
                                CheckOutTime = fields[6],
                                NumOfRatings = int.Parse(fields[7]),
                                SumOfRatings = int.Parse(fields[8]),
                                Address = fields[9],
                                Latitude = double.Parse(fields[10]),
                                Longitude = double.Parse(fields[11]),
                                CreationDate = DateTimeOffset.Now,
                                ModificationDate = null,
                                DistanceFromCityCenter = decimal.Parse(fields[14])
                            };

                            hotels.Add(hotel);
                        }
                    }
                }
                return hotels;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

    }
}

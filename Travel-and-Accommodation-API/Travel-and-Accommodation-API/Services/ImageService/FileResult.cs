namespace Travel_and_Accommodation_API.Services.ImageService
{
    public class FileResult
    {
        public MemoryStream? Stream { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
    }
}

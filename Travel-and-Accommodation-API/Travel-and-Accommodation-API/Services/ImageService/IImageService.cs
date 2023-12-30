namespace Travel_and_Accommodation_API.Services.ImageService
{
    public interface IImageService
    {
        public Task AddFileAsync(FileUpload file, string fileName);
        public Task<FileResult> GetFileAsync(string fileName);
        public Task DeleteFileAsync(string fileName);
        public Task UpdateFile(string fileName, FileUpload file);
        public Task AddMultipleFilesAsync(IEnumerable<FileUpload> files, IEnumerable<string> fileNames);
        public Task DeleteMultipleFilesAsync(IEnumerable<string> fileNames);
        public Task UpdateMultipleFilesAsync(IEnumerable<FileUpload> newFiles, IEnumerable<string> existingFileNames);

    }
}

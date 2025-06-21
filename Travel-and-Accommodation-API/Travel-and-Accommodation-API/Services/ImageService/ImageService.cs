using Google.Cloud.Storage.V1;

namespace Travel_and_Accommodation_API.Services.ImageService
{
    public class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;
        private readonly string GoogleBucket;
        public ImageService(ILogger<ImageService> logger,IConfiguration configuration)
        {
            GoogleBucket = configuration["ImageServiceConfig:GoogleBucketString"] ??
                throw new ArgumentNullException(nameof(GoogleBucket));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddFileAsync(FileUpload file, string fileName)
        {
            try
            {
                using (var storageClient = StorageClient.Create())
                using (var stream = new MemoryStream(file.File))
                {
                    var obj = await storageClient.UploadObjectAsync(
                        GoogleBucket,
                        fileName,
                        file.Type,
                        stream);
                }

                _logger.LogInformation($"File '{fileName}' added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding file '{fileName}': {ex.Message}");
                throw;
            }
        }

        public async Task<FileResult> GetFileAsync(string fileName)
        {
            try
            {
                using (var storageClient = StorageClient.Create())
                {
                    var stream = new MemoryStream();
                    var obj = await storageClient.DownloadObjectAsync(GoogleBucket,
                        fileName, stream);

                    stream.Position = 0;

                    var fileResult = new FileResult
                    {
                        Stream = stream,
                        Name = obj.Name,
                        ContentType = obj.ContentType
                    };

                    _logger.LogInformation($"File '{fileName}' retrieved successfully.");
                    return fileResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving file '{fileName}': {ex.Message}");
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
            try
            {
                using (var storageClient = StorageClient.Create())
                {
                    await storageClient.DeleteObjectAsync(GoogleBucket, fileName);
                }

                _logger.LogInformation($"File '{fileName}' deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting file '{fileName}': {ex.Message}");
                throw;
            }
        }

        public async Task UpdateFile(string fileName, FileUpload file)
        {
            try
            {
                try
                {
                    await DeleteFileAsync(fileName);
                }
                catch (Exception deleteException)
                {
                    _logger.LogInformation($"There is no file with the given {fileName}, the new file will just be inserted instead of update the original file");
                }
                await AddFileAsync(file, fileName);
                _logger.LogInformation("File was inserted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while updating file {fileName}");
                throw;
            }
        }

        public async Task AddMultipleFilesAsync(IEnumerable<FileUpload> files, IEnumerable<string> fileNames)
        {
            try
            {
                if (files == null || fileNames == null || !files.Any() || !fileNames.Any())
                {
                    _logger.LogWarning("No files or file names provided for adding multiple files.");
                    throw new Exception("No files or file names provided for adding multiple files.");
                }

                using (var storageClient = StorageClient.Create())
                {
                    var tasks = files.Zip(fileNames, async (file, fileName) =>
                    {
                        using (var stream = new MemoryStream(file.File))
                        {
                            var obj = await storageClient.UploadObjectAsync(
                                GoogleBucket,
                                fileName,
                                file.Type,
                                stream);
                        }
                        _logger.LogInformation($"File '{fileName}' added successfully.");
                    });

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding multiple files: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteMultipleFilesAsync(IEnumerable<string> fileNames)
        {
            try
            {
                if (fileNames == null || !fileNames.Any())
                {
                    _logger.LogWarning("No file names provided for deleting multiple files.");
                    throw new Exception("File list to be deleted is empty");
                }

                using (var storageClient = StorageClient.Create())
                {
                    var tasks = fileNames.Select(async fileName =>
                    {
                        await storageClient.DeleteObjectAsync(GoogleBucket, fileName);
                        _logger.LogInformation($"File '{fileName}' deleted successfully.");
                    });

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting multiple files: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateMultipleFilesAsync(IEnumerable<FileUpload> newFiles, IEnumerable<string> existingFileNames)
        {
            try
            {
                if (newFiles == null || existingFileNames == null || !newFiles.Any() || !existingFileNames.Any())
                {
                    _logger.LogWarning("No new files or existing file names provided for updating multiple files.");
                    throw new Exception("No new files or existing file names provided for updating multiple files.");
                }

                using (var storageClient = StorageClient.Create())
                {
                    await DeleteMultipleFilesAsync(existingFileNames);
                    var tasks = newFiles.Zip(existingFileNames, async (file, fileName) =>
                    {
                        using (var stream = new MemoryStream(file.File))
                        {
                            var obj = await storageClient.UploadObjectAsync(
                                GoogleBucket,
                                fileName,
                                file.Type,
                                stream);
                        }
                        _logger.LogInformation($"File '{fileName}' updated successfully.");
                    });

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating multiple files: {ex.Message}");
                throw;
            }
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace imageresizer.Storage
{
    public class StorageService
    {
        private readonly CloudBlobClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StorageService> _logger;

        public StorageService(CloudBlobClient client, IConfiguration configuration, ILogger<StorageService> logger)
        {
            _client = client;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool, CloudBlockBlob)> TryGetFile(string name)
        {
            return await TryGetFile(_configuration["StorageAccount:Container"], name);
        }

        public async Task<(bool, CloudBlockBlob)> TryGetFileCached(string name)
        {
            return await TryGetFile(_configuration["StorageAccount:ContainerResized"], name);
        }

        public async Task<(bool, CloudBlockBlob)> TryUploadToCache(string name, byte[] imageContent, string mimeType)
        {
            try
            {
                var cacheBlob = GetFile(_configuration["StorageAccount:ContainerResized"], name);
                await cacheBlob.UploadFromByteArrayAsync(imageContent, 0, imageContent.Length);

                cacheBlob.Properties.ContentType = mimeType;
                await cacheBlob.SetPropertiesAsync();

                return (true, cacheBlob);
            }
            catch(Exception error)
            {
                _logger.LogError(error, $"Could not upload cached file {name}");
            }

            return (false, null);
        }

        private async Task<(bool, CloudBlockBlob)> TryGetFile(string containerName, string name)
        {
            try
            {
                var blob = GetFile(containerName, name);

                if (await blob.ExistsAsync())
                {
                    return (true, blob);
                }
            }
            catch (Exception error)
            {
                _logger.LogError(error, $"Could not download cached file {name}");
            }

            return (false, null);
        }

        private CloudBlockBlob GetFile(string containerName, string blobName)
        {
            //http://127.0.0.1:10000/devstoreaccount1/images/image.big.jpg
            var container = _client.GetContainerReference(containerName);
            return container.GetBlockBlobReference(blobName);
        }

        public async Task<byte[]> GetBlobBytes(CloudBlockBlob blob)
        {
            using (var memory = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(memory);
                return memory.ToArray();
            }
        }
    }
}
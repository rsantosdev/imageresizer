using System;
using imageresizer.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace imageresizer.Configuration
{
    public static class StorageConfiguration
    {
        public static void ConfigureStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(factory => CreateStorageAccount(configuration));
            services.AddSingleton(CreateStorageClient);
            services.AddSingleton<StorageService>();
        }

        private static CloudStorageAccount CreateStorageAccount(IConfiguration configuration)
        {
            return CloudStorageAccount.Parse(configuration["StorageAccount:ConnectionString"]);
        }

        private static CloudBlobClient CreateStorageClient(IServiceProvider factory)
        {
            var account = factory.GetService<CloudStorageAccount>();
            return account.CreateCloudBlobClient();
        }
    }
}
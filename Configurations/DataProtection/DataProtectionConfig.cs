using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.DataProtection;

namespace VoxDocs.Configurations
{
    public static class DataProtectionConfig
    {
        public static IServiceCollection AddCustomDataProtectionAndBlobService(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            var blobConn = configuration["AzureBlobStorage:ConnectionString"];
            
            if (string.IsNullOrEmpty(blobConn))
            {
                throw new InvalidOperationException("Azure Blob Storage connection string não encontrada");
            }

            // Registra o BlobServiceClient para injeção de dependência
            services.AddSingleton(_ => new BlobServiceClient(blobConn));

            // Opção para desenvolvimento local
            if (environment.IsDevelopment() || 
                configuration.GetValue<bool>("DataProtection:UseLocalStorage"))
            {
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(Path.GetTempPath()))
                    .SetApplicationName("VoxDocs");
                
                return services;
            }

            // Configuração para produção
            try
            {
                var container = configuration["AzureBlobStorage:Containers:DataProtection"];
                var blobName = configuration["DataProtection:BlobName"];

                if (string.IsNullOrEmpty(container) || string.IsNullOrEmpty(blobName))
                {
                    throw new InvalidOperationException("Configurações do Blob Storage para Data Protection não encontradas");
                }

                var containerClient = new BlobContainerClient(blobConn, container);
                containerClient.CreateIfNotExists(PublicAccessType.None);

                services.AddDataProtection()
                       .SetApplicationName("VoxDocs")
                       .PersistKeysToAzureBlobStorage(containerClient.GetBlobClient(blobName));

                return services;
            }
            catch (Exception ex)
            {
                // Fallback seguro
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(Path.GetTempPath()))
                    .SetApplicationName("VoxDocs");
                
                return services;
            }
        }
    }
}
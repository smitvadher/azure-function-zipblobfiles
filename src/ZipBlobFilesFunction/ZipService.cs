using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ZipBlobFilesFunction
{
    /// <summary>
    /// Service for zipping files and storing them in Azure Blob Storage.
    /// </summary>
    public class ZipService
    {
        #region Fields

        private const int CompressionLevel = 6;
        private readonly ILogger<ZipService> _logger;
        private readonly AppSettings _blobConfig;

        #endregion

        #region Ctor

        public ZipService(ILogger<ZipService> logger, IOptions<AppSettings> options)
        {
            if (string.IsNullOrEmpty(options.Value.ConnectionString))
                throw new Exception("AppSettings.ConnectionString is not valid");

            if (string.IsNullOrEmpty(options.Value.ContainerName))
                throw new Exception("AppSettings.ContainerName is not valid");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blobConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Generates a unique zip file name.
        /// </summary>
        private static string GetZipFileName()
        {
            return $"zip-files/{DateTime.UtcNow:yyyyMMddHHmmss}.{Guid.NewGuid().ToString()[..4]}.zip";
        }

        /// <summary>
        /// Opens a stream to write a zip file in Azure Blob Storage.
        /// </summary>
        private async Task<Stream> OpenZipFileStreamAsync(string containerName,
            string zipFilename,
            CancellationToken cancellationToken)
        {
            var blobClient = new BlockBlobClient(_blobConfig.ConnectionString, containerName, zipFilename);
            var options = new BlockBlobOpenWriteOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "application/zip"
                }
            };

            return await blobClient.OpenWriteAsync(true, options, cancellationToken);
        }

        /// <summary>
        /// Adds a file entry to the zip stream.
        /// </summary>
        private async Task ZipFileEntryAsync(ZipOutputStream zipFileOutputStream, string filePath, CancellationToken cancellationToken)
        {
            var blockBlobClient = new BlockBlobClient(_blobConfig.ConnectionString, _blobConfig.ContainerName, filePath);
            var properties = await blockBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            var fileName = Path.GetFileName(blockBlobClient.Name);
            var zipEntry = new ZipEntry(fileName) { Size = properties.Value.ContentLength };

            await zipFileOutputStream.PutNextEntryAsync(zipEntry, cancellationToken);
            await blockBlobClient.DownloadToAsync(zipFileOutputStream, cancellationToken);
            await zipFileOutputStream.CloseEntryAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes the zip file from Azure Blob Storage if it exists.
        /// </summary>
        private async Task DeleteZipFileIfExistsAsync(string zipFileName, CancellationToken cancellationToken)
        {
            var blockBlobClient = new BlockBlobClient(_blobConfig.ConnectionString, _blobConfig.ContainerName, zipFileName);
            await blockBlobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Zips a collection of files.
        /// </summary>
        /// <param name="filePaths">The paths of the files to be zipped.</param>
        /// <param name="cancellationToken">Cancellation token to propagate notifications that the operation should be cancelled.</param>
        /// <returns>The name of the created zip file.</returns>
        public async Task<string> ZipFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken)
        {
            var zipFileName = GetZipFileName();
            var sw = Stopwatch.StartNew();

            _logger.LogInformation($"Using level {CompressionLevel} compression");

            try
            {
                await using (var blobStream = await OpenZipFileStreamAsync(_blobConfig.ContainerName, zipFileName, cancellationToken))
                await using (var zipOutputStream = new ZipOutputStream(blobStream) { IsStreamOwner = false })
                {
                    zipOutputStream.SetLevel(CompressionLevel);

                    foreach (var filePath in filePaths)
                    {
                        await ZipFileEntryAsync(zipOutputStream, filePath, cancellationToken);
                    }
                }

                sw.Stop();

                _logger.LogInformation($"Created zip [{zipFileName}] in {sw.ElapsedMilliseconds} ms.");

                return zipFileName;
            }
            catch (Exception)
            {
                await DeleteZipFileIfExistsAsync(zipFileName, cancellationToken);
                throw;
            }
        }

        #endregion
    }
}

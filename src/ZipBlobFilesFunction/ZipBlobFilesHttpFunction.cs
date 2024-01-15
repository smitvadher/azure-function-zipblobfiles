using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ZipBlobFilesFunction
{
    public class ZipBlobFilesHttpFunction
    {
        #region Fields

        private readonly ZipService _zipService;

        #endregion

        #region Ctor

        public ZipBlobFilesHttpFunction(ZipService zipService)
        {
            _zipService = zipService;
        }

        #endregion

        #region Function

        [FunctionName("ZipBlobFilesHttpFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "POST", Route = null)] ZipFileRequest request,
            HttpRequest _,
            ILogger log,
            CancellationToken cancellationToken)
        {
            if (!ModelValidation.TryValidateObject(request, out var validationResults))
            {
                return new BadRequestObjectResult(new
                {
                    errors = validationResults.Select(x => x.ErrorMessage)
                });
            }

            log.LogInformation($"Zipping {request.FilePaths.Count} files.");

            var zipFilePath = await _zipService.ZipFilesAsync(request.FilePaths, cancellationToken);

            log.LogInformation($"Zipped {request.FilePaths.Count} files.");

            return new JsonResult(new { zipFilePath });
        }

        #endregion
    }
}

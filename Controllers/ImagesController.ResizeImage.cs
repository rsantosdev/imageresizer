using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using imageresizer.ControllerResults;
using imageresizer.Converter;
using imageresizer.Models;
using imageresizer.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace imageresizer.Controllers
{
    public partial class ImagesController : ControllerBase
    {
        [HttpGet("api/images/resize")]
        public async Task<IActionResult> ResizeImage(
            [FromQuery]ResizeRequestModel requestModel,
            [FromServices]IHostingEnvironment env,
            [FromServices]StorageService storage,
            [FromServices]ImageConverter converter
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var options = ConversionOptionsFactory.FromResizeRequest(requestModel);

            (var cacheExists, var cacheFile) = await storage.TryGetFileCached(options.GetCacheKey());
            if (cacheExists)
            {
                // validar Etag
                if (IsEtagNotModified(Request, cacheFile.Properties.ETag))
                {
                    return new NotModifiedResult(
                        cacheFile.Properties.LastModified.GetValueOrDefault().UtcDateTime, 
                        cacheFile.Properties.ETag);
                }
                
                var cacheContent = await storage.GetBlobBytes(cacheFile);
                return File(cacheContent, cacheFile.Properties.ContentType,
                    cacheFile.Properties.LastModified.GetValueOrDefault().UtcDateTime,
                    new EntityTagHeaderValue(cacheFile.Properties.ETag));
            }

            (var fileExists, var blobFile) = await storage.TryGetFile(requestModel.Name);
            if (fileExists == false)
            {
                return NotFound();
            }

            var imageSource = await storage.GetBlobBytes(blobFile);
            var result = await converter.Convert(imageSource, options);

            if (result.Length == 0)
            {
                return BadRequest("Could not convert file.");
            }

            // Salvar a imagem convertida em um blob separado
            (var uploadOk, var savedFile) = await storage.TryUploadToCache(options.GetCacheKey(),
                result, options.TargetMimeType);

            return File(result, savedFile.Properties.ContentType,
                savedFile.Properties.LastModified.GetValueOrDefault().UtcDateTime,
                new EntityTagHeaderValue(savedFile.Properties.ETag));
        }

        static bool IsEtagNotModified(HttpRequest request, string etag)
        {
            var requestHeaders = request?.GetTypedHeaders();

            return requestHeaders?.IfNoneMatch != null &&
                requestHeaders.IfNoneMatch.Contains(new EntityTagHeaderValue(etag));
        }
    }
}
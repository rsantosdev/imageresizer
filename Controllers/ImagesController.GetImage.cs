using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace imageresizer.Controllers
{
    public partial class ImagesController : ControllerBase
    {
        [HttpGet("api/images")]
        public async Task<IActionResult> GetImage(
            [FromQuery]string name,
            [FromServices]IHostingEnvironment env
        )
        {
            var filePath = Path.Combine(env.ContentRootPath, "Images", name);
            var fileExists = System.IO.File.Exists(filePath);
            if (fileExists == false)
            {
                return NotFound();
            }

            var file = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(file, "image/jpeg");
        }
    }
}
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace imageresizer.ControllerResults
{
    public class NotModifiedResult : ActionResult
    {
        const int OneYearDuration = 31536000;

        private readonly DateTime _lastModified;
        private readonly string _etag;

        public NotModifiedResult(DateTime lastModified, string etag)
        {
            _lastModified = lastModified;
            _etag = etag;
        }

        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.HttpContext.Response.ContentType = null;
            context.HttpContext.Response.Body = null;

            context.HttpContext.Response.StatusCode = 304;
            context.HttpContext.Response.Headers[HeaderNames.ETag] = _etag;
            context.HttpContext.Response.Headers[HeaderNames.LastModified] = _lastModified.ToString("R");
            context.HttpContext.Response.Headers[HeaderNames.Vary] = "Accept";
            context.HttpContext.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={OneYearDuration}";
            context.HttpContext.Response.Headers[HeaderNames.ContentLength] = "0";
        }
    }
}
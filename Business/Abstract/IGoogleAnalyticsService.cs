using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Business.Abstract
{
    public interface IGoogleAnalyticsService
    {
        Task TrackEventAsync(string eventName, object eventParams, HttpContext httpContext, string? userId = null);
    }
}

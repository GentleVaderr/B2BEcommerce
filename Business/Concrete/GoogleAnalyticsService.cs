using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using Business.Abstract;
using Microsoft.Identity.Client;
using Entities.DTOs;


namespace Business.Concrete
{
    public class GoogleAnalyticsService : IGoogleAnalyticsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GoogleAnalyticsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        public async Task TrackEventAsync(string eventName, object eventParams, HttpContext httpContext, string? userId = null)
        {
            string clientId = Guid.NewGuid().ToString();
            if (httpContext != null && httpContext.Request.Cookies.TryGetValue("_ga", out string gaCookie))
            { 
                var cookieParts = gaCookie.Split('.');
                if (cookieParts.Length >= 4)
                {
                    clientId = $"{cookieParts[2]}.{cookieParts[3]}";
                }
            }

            var ga4Payload = new Ga4PayloadModel
            {
                ClientId = clientId,
                UserId = userId,
                Events = new List<Ga4EventModel>
                {
                    new Ga4EventModel
                    {
                        Name = eventName,
                        Params = eventParams
                    }
                }
            };

            string? baseUrl = _configuration["GoogleAnalytics:BaseUrl"];
            string? measurementId = _configuration["GoogleAnalytics:MeasurementId"];
            string? apiSecret = _configuration["GoogleAnalytics:ApiSecret"];
            string ga4Url = $"{baseUrl}?measurement_id={measurementId}&api_secret={apiSecret}";

            try
            {
                var client = _httpClientFactory.CreateClient();
                await client.PostAsJsonAsync(ga4Url, ga4Payload);
            }
            catch (Exception)
            {

            }
        }
    }
}

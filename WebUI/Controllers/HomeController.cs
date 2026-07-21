using Entities.DTOs;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebUI.Filters;
using WebUI.Models;

namespace WebUI.Controllers
{
    [CustomerAuth]
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TrackClick([FromBody] ButtonClickDto clickData)
        {
            string clientId = Guid.NewGuid().ToString();
            if (Request.Cookies.TryGetValue("_ga", out string? gaCookie))
            {
                var cookieParts = gaCookie.Split('.');
                if (cookieParts.Length >= 4)
                {
                    clientId = $"{cookieParts[2]}.{cookieParts[3]}";
                }
            }

            var ga4Payload = new
            {
                client_id = clientId,
                events = new[]
                {
            new
            {
                name = "button_click",
                @params = new
                {
                    debug_mode = 1,
                    button_text = clickData.ButtonText,
                    button_id = clickData.ButtonId,
                    page_url = clickData.PageUrl
                }
            }
        }
            };

            string measurementId = "G-X8H3TG9MKJ"; // Kimliğin
            string apiSecret = "ckq2ILLbQnmfSOn_9vrRQQ";       // Gizli anahtarın
            string ga4Url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";

            try
            {
                var client = _httpClientFactory.CreateClient();
                await client.PostAsJsonAsync(ga4Url, ga4Payload);
            }
            catch (Exception) { /* Sessizce yut */ }

            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            string errorMessage = exceptionHandlerPathFeature?.Error?.Message ?? "Bilinmeyen Hata";
            string errorPath = exceptionHandlerPathFeature?.Path ?? "Bilinmeyen Sayfa";

            // --- GA4 EXCEPTION (HATA) ENTEGRASYONU BAŞLANGICI ---
            string clientId = Guid.NewGuid().ToString();
            if (Request.Cookies.TryGetValue("_ga", out string? gaCookie))
            {
                var cookieParts = gaCookie.Split('.');
                if (cookieParts.Length >= 4)
                {
                    clientId = $"{cookieParts[2]}.{cookieParts[3]}";
                }
            }

            var ga4Payload = new
            {
                client_id = clientId,
                events = new[]
                {
            new
            {
                name = "exception", // GA4'ün standart hata takip etkinliği
                @params = new
                {
                    description = $"Yol: {errorPath} | Hata: {errorMessage}",
                    fatal = 1 // 1 = Sistem hatası (Kullanıcının işini bölen kritik hata)
                }
            }
        }
            };

            string measurementId = "G-X8H3TG9MKJ"; // Kendi kimliğini yaz
            string apiSecret = "ckq2ILLbQnmfSOn_9vrRQQ";       // Kendi gizli anahtarını yaz

            string ga4Url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";

            try
            {
                var client = _httpClientFactory.CreateClient();
                await client.PostAsJsonAsync(ga4Url, ga4Payload);
            }
            catch (Exception)
            {
                // Hata raporlama sisteminin kendisi patlarsa sessizce yutuyoruz :)
            }
            // --- GA4 ENTEGRASYONU BİTİŞİ ---

            // Mevcut MVC Error View'ına yönlendirme (Senin kodunda farklı olabilir, onu koru)
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

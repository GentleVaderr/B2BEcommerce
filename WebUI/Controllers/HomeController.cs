using Business.Abstract;
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
        private readonly IConfiguration _configuration;
        private readonly IGoogleAnalyticsService _gaService;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IGoogleAnalyticsService gaService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _gaService = gaService;
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
            if (clickData == null) return BadRequest();

            // GA4'e button_click olayını dinamik parametrelerle yolluyoruz
            await _gaService.TrackEventAsync("button_click", new
            {
                debug_mode = 1,
                button_text = clickData.ButtonText,
                button_id = clickData.ButtonId,
                page_url = clickData.PageUrl
            }, HttpContext);

            return Ok(); // JavaScript'e işlemin başarılı olduğunu bildiriyoruz
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            string errorMessage = exceptionHandlerPathFeature?.Error?.Message ?? "Bilinmeyen Hata";
            string errorPath = exceptionHandlerPathFeature?.Path ?? "Bilinmeyen Sayfa";

            await _gaService.TrackEventAsync("exception", new
            {
                debug_mode = 1,
                error_message = errorMessage,
                error_path = errorPath
            }, HttpContext);

            // Mevcut MVC Error View'ına yönlendirme (Senin kodunda farklı olabilir, onu koru)
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

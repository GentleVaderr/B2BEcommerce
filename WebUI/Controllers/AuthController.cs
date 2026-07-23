using Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IGoogleAnalyticsService _gaService;

        public AuthController(IUserService userService, IHttpClientFactory httpClientFactory, IConfiguration configuration, IGoogleAnalyticsService gaService)
        {
            _userService = userService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _gaService = gaService;
        }

        // Giriş Sayfasını Getirir (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Formdan Gelen Bilgileri Kontrol Eder (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            string hashedPassword = Business.Utilities.HashHelper.CreateMD5(password);
            var user = _userService.GetAll().FirstOrDefault(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user != null)
            {
                HttpContext.Session.SetInt32("CurrentUserId", user.Id);
                HttpContext.Session.SetString("CurrentUserFullName", $"{user.FullName}");
                HttpContext.Session.SetString("CurrentUserRole", user.Role);

                if (user.Role == "Admin")
                {
                    string safeFullName = Uri.EscapeDataString($"{user.FullName}");
                    string safeRole = Uri.EscapeDataString(user.Role);

                    return Redirect($"https://localhost:7024/Auth/AdminTransfer?fullName={safeFullName}&role={safeRole}");
                }
                else
                {
                    return RedirectToAction("Index", "Product");
                }
            }

            ViewBag.ErrorMessage = "E-posta adresi veya şifre hatalı!";

            await _gaService.TrackEventAsync("login", new { debug_mode = 1, method = "Email_Password", user_role = user?.Role }, HttpContext, user?.Id.ToString());

            return View();
        }

        // Çıkış Yapma
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Hafızadaki tüm kullanıcı bilgilerini temizle
            return RedirectToAction("Login", "Auth"); // Giriş ekranına geri gönder
        }
    }
}
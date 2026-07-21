using Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IUserService userService, IHttpClientFactory httpClientFactory)
        {
            _userService = userService;
            _httpClientFactory = httpClientFactory;
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

            // --- GA4 LOGIN ENTEGRASYONU BAŞLANGICI ---
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
                user_id = user?.Id.ToString(), // Kullanıcı ID'si cihazlar arası izleme (Cross-Device) için eklendi
                events = new[]
                {
                new
                {
                    name = "login",
                    @params = new
                    {
                        method = "Email_Password",
                        user_role = user?.Role // Ekstra: Sisteme admin mi yoksa normal üye mi girdiğini GA4'e bildiriyoruz
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
               
            }
            // --- GA4 ENTEGRASYONU BİTİŞİ ---

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
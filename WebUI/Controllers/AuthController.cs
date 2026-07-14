using Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // Giriş Sayfasını Getirir (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Formdan Gelen Bilgileri Kontrol Eder (POST)
        [HttpPost]
        public IActionResult Login(string email, string password)
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
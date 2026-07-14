using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Mvc;
using WebAdmin.Filters;

namespace WebAdmin.Controllers
{
    [AdminAuth]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Kullanıcı Listesi
        public IActionResult Index()
        {
            var users = _userService.GetAll();
            return View(users);
        }

        // Kullanıcı Ekleme Formu (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Kullanıcıyı Veritabanına Kaydetme (POST)
        [HttpPost]
        public IActionResult Create(User user)
        {
            // Veritabanına kaydetmeden hemen önce kullanıcının girdiği şifreyi MD5'e çeviriyoruz
            user.PasswordHash = Business.Utilities.HashHelper.CreateMD5(user.PasswordHash);

            _userService.Add(user);
            return RedirectToAction("Index");
        }

        // 1. KULLANICI SİLME METODU
        public IActionResult Delete(int id)
        {
            var user = _userService.GetAll().FirstOrDefault(u => u.Id == id);

            if (user != null)
            {
                // Güvenlik Önlemi: Kendi kendini silmeyi engelleme
                var currentAdminId = HttpContext.Session.GetInt32("CurrentUserId");
                if (user.Id == currentAdminId)
                {
                    return RedirectToAction("Index");
                }

                _userService.Delete(user);
            }
            return RedirectToAction("Index");
        }

        // 2. YETKİ DEĞİŞTİRME SAYFASINI AÇAN METOT (GET)
        [HttpGet]
        public IActionResult EditRole(int id)
        {
            var user = _userService.GetAll().FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // 3. YENİ YETKİYİ KAYDEDEN METOT (POST)
        [HttpPost]
        public IActionResult EditRole(Entities.Concrete.User user)
        {
            // Önce kullanıcının mevcut halini veritabanından buluyoruz
            var existingUser = _userService.GetAll().FirstOrDefault(u => u.Id == user.Id);

            if (existingUser != null)
            {
                existingUser.Role = user.Role;

                _userService.Update(existingUser);
            }

            return RedirectToAction("Index");
        }
    }
}
using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace WebAdmin.Controllers
{
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
            _userService.Add(user);
            return RedirectToAction("Index"); // Kayıt bitince listeye dön
        }
    }
}
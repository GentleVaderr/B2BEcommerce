using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Mvc;
using WebAdmin.Filters;

namespace WebUI.Controllers
{
    [AdminAuth]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // --- READ (LİSTELEME) ---
        public IActionResult Index()
        {
            var products = _productService.GetAll();
            return View(products);
        }

        // --- CREATE (EKLEME) ---
        // 1. Adım:(GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 2. Adım:(POST)
        [HttpPost]
        public IActionResult Create(Product product)
        {
            // İleride burada ModelState doğrulaması yapılabilir
            _productService.Add(product);

            return RedirectToAction("Index");
        }

        // 1. AŞAMA: Düzenlenecek ürünü bulup form sayfasına gönderen metot (Ekrana getirme)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // ID'sine göre ilgili ürünü veritabanından çekme
            var product = _productService.GetAll().FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // 2. AŞAMA: Formdan gelen güncel bilgileri veritabanına kaydeden metot (Güncelleme)
        [HttpPost]
        public IActionResult Edit(Entities.Concrete.Product product)
        {
            _productService.Update(product);

            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var product = _productService.GetAll().FirstOrDefault(p => p.Id == id);

            if (product != null)
            {
                _productService.Delete(product);
            }

            return RedirectToAction("Index");
        }
    }
}
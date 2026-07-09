using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
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
        // 1. Adım: Sadece boş HTML formunu ekrana getirir (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 2. Adım: Kullanıcı formu doldurup 'Kaydet'e basınca çalışır (POST)
        [HttpPost]
        public IActionResult Create(Product product)
        {
            // İleride burada ModelState doğrulaması yapılabilir
            _productService.Add(product);

            // Ürün başarıyla eklenince, kullanıcıyı tekrar liste sayfasına (Index) gönderiyoruz.
            return RedirectToAction("Index");
        }
    }
}
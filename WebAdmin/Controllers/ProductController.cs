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

            // Ürün başarıyla eklenince, kullanıcıyı tekrar liste sayfasına (Index) gönderiyoruz.
            return RedirectToAction("Index");
        }

        // 1. AŞAMA: Düzenlenecek ürünü bulup form sayfasına gönderen metot (Ekrana getirme)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // ID'sine göre ilgili ürünü veritabanından çekiyoruz
            var product = _productService.GetAll().FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound(); // Eğer ürün yoksa hata sayfasına yönlendir
            }

            return View(product); // Ürünü bulduk, içindeki bilgilerle birlikte görünüme gönderiyoruz
        }

        // 2. AŞAMA: Formdan gelen güncel bilgileri veritabanına kaydeden metot (Güncelleme)
        [HttpPost]
        public IActionResult Edit(Entities.Concrete.Product product)
        {
            // Ürünü güncelliyoruz
            _productService.Update(product);

            // İşlem bitince ürünler listesine geri dönüyoruz
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            // 1. Silinecek ürünü veritabanından buluyoruz
            var product = _productService.GetAll().FirstOrDefault(p => p.Id == id);

            // 2. Eğer böyle bir ürün varsa silme işlemini yapıyoruz
            if (product != null)
            {
                _productService.Delete(product);
            }

            // 3. İşlem bitince sayfayı yenilemek için listeye geri dönüyoruz
            return RedirectToAction("Index");
        }
    }
}
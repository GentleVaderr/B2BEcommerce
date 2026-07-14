using Business.Abstract;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using Entities.DTOs;

namespace WebUI.Controllers
{
    [CustomerAuth]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            var rawProducts = _productService.GetAll();

            var productDto = rawProducts.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Stock = p.Stock

            }).ToList();

            return View(productDto);
        }
    }
}

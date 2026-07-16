using Business.Abstract;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using WebAdmin.Filters;

namespace WebAdmin.Controllers
{
    [AdminAuth]
    public class OrderController : Controller
    {

        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IProductService _productService;

        public OrderController(IOrderService orderService, IOrderDetailService orderDetailService, IProductService productService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _productService = productService;
        }

        // Siparişleri Listeleme Sayfası
        public IActionResult Index()
        {
            var orders = _orderService.GetAll();

            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int orderId, string status)
        {
            var order = _orderService.GetAll().FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = status;
                
                _orderService.Update(order);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var mainOrder = _orderService.GetById(id);
            if (mainOrder == null) return NotFound();
            var orderDetails = _orderDetailService.GetAll().Where(d => d.OrderId == id).ToList();
            var allProducts = _productService.GetAll();

            var summaryDto = new OrderSummaryDto
            {
                Id = mainOrder.Id,
                UserId = mainOrder.UserId,
                OrderDate = mainOrder.OrderDate,
                Status = mainOrder.Status,
                TotalPrice = mainOrder.TotalPrice,
            };

            foreach (var item in orderDetails)
            {
                var currentProduct = allProducts.FirstOrDefault(p => p.Id == item.ProductId);
                if (currentProduct != null)
                {
                    summaryDto.OrderDetails.Add(new OrderDetailDto
                    {
                        ProductName = currentProduct.Name,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity
                    });
                }
            }

            return View(summaryDto);
        }
    }
}
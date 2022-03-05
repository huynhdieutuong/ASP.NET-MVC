using AppMVC.Areas.Product.Models;
using AppMVC.Areas.Product.Service;
using AppMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppMVC.Areas.Product.Controllers
{
    [Area("Product")]
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly AppDbContext _context;
        private readonly CartService _cartService;

        public CartController(ILogger<CartController> logger, AppDbContext context, CartService cartService)
        {
            _logger = logger;
            _context = context;
            _cartService = cartService;
        }

        [Route("/cart")]
        public IActionResult Index()
        {
            var cart = _cartService.GetCartItems();
            return View(cart);
        }

        [Route("/cart/{productId}/add-item")]
        public IActionResult AddToCart(int? productId)
        {
            if (productId == null) return NotFound("ProductId not found");

            var product = _context.Products.Find(productId);
            if (product == null) return NotFound("Product not found");

            var cart = _cartService.GetCartItems();
            var cartItem = cart.Find(item => item.Product.Id == productId);

            if (cartItem == null)
            {
                cart.Add(new CartItem()
                {
                    Quantity = 1,
                    Product = product
                });
            } else
            {
                cartItem.Quantity++;
            }

            _cartService.SaveCartSession(cart);
            return RedirectToAction("Index");
        }

        [Route("/cart/{productId}/delete-item")]
        public IActionResult DeleteItem(int? productId)
        {
            if (productId == null) return NotFound("ProductId not found");

            var product = _context.Products.Find(productId);
            if (product == null) return NotFound("Product not found");

            var cart = _cartService.GetCartItems();
            var cartItem = cart.Find(item => item.Product.Id == productId);
            
            if (cartItem != null) cart.Remove(cartItem);
            _cartService.SaveCartSession(cart);
            return RedirectToAction("Index");
        }

        [Route("/cart/update-item")]
        [HttpPost]
        public IActionResult UpdateItem(int? productId, int quantity)
        {
            if (productId == null) return NotFound("ProductId not found");

            var product = _context.Products.Find(productId);
            if (product == null) return NotFound("Product not found");

            var cart = _cartService.GetCartItems();
            var cartItem = cart.Find(item => item.Product.Id == productId);

            if (cartItem != null) cartItem.Quantity = quantity;
            _cartService.SaveCartSession(cart);

            return Ok();
        }

        [Route("/checkout")]
        public IActionResult Checkout()
        {
            _cartService.ClearCart();
            return Content("Order success");
        }
    }
}

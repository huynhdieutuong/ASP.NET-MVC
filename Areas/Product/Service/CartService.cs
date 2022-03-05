using AppMVC.Areas.Product.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AppMVC.Areas.Product.Service
{
    public class CartService
    {
        public const string CARTKEY = "cart";
        private readonly IHttpContextAccessor _context;
        private readonly HttpContext httpContext;

        public CartService(IHttpContextAccessor context)
        {
            _context = context;
            httpContext = context.HttpContext;
        }

        public List<CartItem> GetCartItems()
        {
            var session = httpContext.Session;
            string jsonCart = session.GetString(CARTKEY);
            if (!string.IsNullOrEmpty(jsonCart))
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(jsonCart);
            }
            return new List<CartItem>();
        }

        public void ClearCart()
        {
            var session = httpContext.Session;
            session.Remove(CARTKEY);
        }

        public void SaveCartSession(List<CartItem> list)
        {
            var session = httpContext.Session;
            string jsonCart = JsonConvert.SerializeObject(list);
            session.SetString(CARTKEY, jsonCart);
        }
    }
}

using System.Collections.Generic;
using AppMVC.Models;

namespace AppMVC.Services
{
    // 3.2 Create Service
    public class ProductService : List<ProductModel>
    {
        public ProductService()
        {
            this.AddRange(new ProductModel[] {
                new ProductModel() { Id = 1, Name = "Iphone X", Price = 1000 },
                new ProductModel() { Id = 2, Name = "Samsung 10", Price = 600 },
                new ProductModel() { Id = 3, Name = "Sony 11", Price = 900 },
                new ProductModel() { Id = 4, Name = "Nokia 3310", Price = 200 },
            });
        }
    }
}
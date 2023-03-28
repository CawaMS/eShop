﻿using eShop.Data;
using eShop.Interfaces;
using eShop.Models;

namespace eShop.Services
{
    public class ProductServiceDB : IProductService
    {
        private readonly eShopContext _context;

        public ProductServiceDB(eShopContext context) {  _context = context; }

        public Task<List<Product>> GetAllProductsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            var product = await Task.Run(() => _context.Product.Where(product => product.Id == productId).FirstOrDefault());

            return product;
            
        }
    }
}

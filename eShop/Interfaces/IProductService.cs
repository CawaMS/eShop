﻿using eShop.Models;
namespace eShop.Interfaces;

public interface IProductService
{
    Task<Product?> GetProductByIdAsync(int productId);
    IAsyncEnumerable<Product> GetAllProductsAsync();
    Task AddProduct(Product product);
    Task EditProduct(Product product);
    Task DeleteProrduct(int productId);
}

using eShop.Models;
namespace eShop.Interfaces;

public interface IProductService
{
    Task<Product?> GetProductByIdAsync(int productId);
    IAsyncEnumerable<Product> GetAllProductsAsync();
    Task AddProduct(Product product);
    Task UpdateProduct(Product product);
    Task DeleteProduct(int productId);
}

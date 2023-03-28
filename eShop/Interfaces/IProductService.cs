using eShop.Models;
namespace eShop.Interfaces;

public interface IProductService
{
    Task<Product?> GetProductByIdAsync(int productId);
    Task<List<Product>> GetAllProductsAsync();
}

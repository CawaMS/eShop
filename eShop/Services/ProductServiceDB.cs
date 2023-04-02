using eShop.Data;
using eShop.Interfaces;
using eShop.Models;

namespace eShop.Services
{
    public class ProductServiceDB : IProductService
    {
        private readonly eShopContext _context;

        public ProductServiceDB(eShopContext context) 
        {  
            _context = context; 
        }

        public Task AddProduct(Product product)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProrduct(int productId)
        {
            throw new NotImplementedException();
        }

        public Task EditProduct(Product product)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            if (_context.Product == null) throw new Exception("Entity set 'eShopContext.Product'  is null.");
            return await Task.Run(() => _context.Product.ToList());
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            var product = await Task.Run(() => _context.Product.Where(product => product.Id == productId).FirstOrDefault());

            return product;
            
        }
    }
}

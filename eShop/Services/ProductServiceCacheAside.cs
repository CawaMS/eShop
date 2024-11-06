using eShop.Data;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.Text;
using System.Text.Json;
using eShop.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Caching.Hybrid;

namespace eShop.Services
{
    public class ProductServiceCacheAside : IProductService
    {
        private readonly IDistributedCache _cache;
        private readonly eShopContext _context;
        private readonly TelemetryClient _telemetryClient;
        private readonly HybridCache _hybridCache;

        public ProductServiceCacheAside(IDistributedCache cache, eShopContext context, TelemetryClient telemetryClient) 
        { 
            _cache = cache;
            _context = context;
            _telemetryClient = telemetryClient;
        }

        public async Task AddProduct(Product product)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(CacheKeyConstants.AllProductKey);
        }

        public async Task DeleteProduct(int productId)
        {
            if (_context.Product == null)
            {
                throw new Exception("Item not found");
            }
            var product = await _context.Product.FindAsync(productId);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(CacheKeyConstants.AllProductKey);
            await _cache.RemoveAsync(CacheKeyConstants.ProductPrefix + productId);
        }

        public async Task UpdateProduct(Product product)
        {
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                await _cache.RemoveAsync(CacheKeyConstants.AllProductKey);
                await _cache.RemoveAsync(CacheKeyConstants.ProductPrefix + product.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                {
                    throw new Exception("Item not found");
                }
                else
                {
                    throw;
                }
            }
        }

        public async IAsyncEnumerable<Product> GetAllProductsAsync()
        {
            var allProducts = await _hybridCache.GetOrCreateAsync(CacheKeyConstants.AllProductKey, async _ =>
            {
                var products = await _context.Product.ToListAsync();
                return products;
            });

            foreach (var product in allProducts)
            {
                yield return product;
            }

        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
           
            var productById = await _hybridCache.GetOrCreateAsync(CacheKeyConstants.ProductPrefix + productId, async _ =>
            {
                var product = await _context.Product.Where(product => product.Id == productId).FirstOrDefaultAsync();
                return product;
            });

            return productById;
        }

        private bool ProductExists(int id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }


}

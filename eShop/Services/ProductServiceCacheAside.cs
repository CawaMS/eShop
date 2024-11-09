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
        private readonly ILogger _logger;

        public ProductServiceCacheAside(IDistributedCache cache, eShopContext context, TelemetryClient telemetryClient, ILogger<ProductServiceCacheAside> logger, HybridCache hybridCache) 
        { 
            _cache = cache;
            _context = context;
            _telemetryClient = telemetryClient;
            _logger = logger;
            _hybridCache = hybridCache;
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
            Console.WriteLine("Getting all products");
            _logger.LogInformation("Getting all products");
            Console.WriteLine(_hybridCache.ToString());
            _logger.LogInformation(_hybridCache.ToString());
            var allProducts = await _hybridCache.GetOrCreateAsync(CacheKeyConstants.AllProductKey, async _ =>
            {
                if (_context.Product == null)
                {
                    Console.WriteLine("No products found");
                    _logger.LogInformation("No products found");
                    throw new Exception("No products found");
                }
                else {
                    Console.WriteLine(_context.Product.Count());
                    _logger.LogInformation(_context.Product.Count().ToString());
                }
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

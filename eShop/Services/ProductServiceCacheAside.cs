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

namespace eShop.Services
{
    public class ProductServiceCacheAside : IProductService
    {
        private readonly IDistributedCache _cache;
        private readonly eShopContext _context;
        private readonly TelemetryClient _telemetryClient;

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
            using (var operation = _telemetryClient.StartOperation<DependencyTelemetry>("AzureCacheForRedis"))
            {
                var byteArrayFromCache = await _cache.GetAsync(CacheKeyConstants.AllProductKey);
                if (byteArrayFromCache.IsNullOrEmpty())
                {
                    if (_context.Product == null) throw new Exception("Entity set 'eShopContext.Product'  is null.");
                    List<Product> AllProductList = await _context.Product.ToListAsync();
                    var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2));
                    if (AllProductList.IsNullOrEmpty())
                    {
                        yield break;
                    }
                    byte[] AllProductByteArray = await ConvertData<Product>.ObjectListToByteArray(AllProductList);
                    await _cache.SetAsync(CacheKeyConstants.AllProductKey, AllProductByteArray, options);
                    foreach (var product in AllProductList)
                    {
                        yield return product;
                    }
                }
                else 
                {
                    IAsyncEnumerable<Product> productList = ConvertData<Product>.ByteArrayToObjectList(byteArrayFromCache);
                    await foreach (var product in productList)
                    {
                        yield return product;
                    }
                }

                

            }

         }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            var byteArrayFromCache = await _cache.GetAsync(CacheKeyConstants.ProductPrefix + productId);
            if (byteArrayFromCache.IsNullOrEmpty())
            {
                var productById = await Task.Run(() => _context.Product.Where(product => product.Id == productId).FirstOrDefault());
                if (productById == null)
                {
                    return null;
                }
                var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2));
                byte[] ProductByIdByteArray = await ConvertData<Product>.ObjectToByteArray(productById);
                await _cache.SetAsync(CacheKeyConstants.ProductPrefix + productId, ProductByIdByteArray, options);
                return productById;
            }

            return await ConvertData<Product>.ByteArrayToObject(byteArrayFromCache);
        }

        private bool ProductExists(int id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }


}

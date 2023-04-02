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

namespace eShop.Services
{
    public class ProductServiceCacheAside : IProductService
    {
        private readonly IDistributedCache _cache;
        private readonly eShopContext _context;

        public ProductServiceCacheAside(IDistributedCache cache, eShopContext context) 
        { 
            _cache = cache;
            _context = context;
        }

        public async Task AddProduct(Product product)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(CacheKeyConstants.AllProductKey);
        }

        public async Task DeleteProrduct(int productId)
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

        public async Task EditProduct(Product product)
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

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var bytesFromCache = await _cache.GetAsync(CacheKeyConstants.AllProductKey);
            if (bytesFromCache.IsNullOrEmpty())
            {
                if (_context.Product == null) throw new Exception("Entity set 'eShopContext.Product'  is null.");
                Console.WriteLine("Fetching from redis");
                List<Product> AllProductList = await Task.Run(() => _context.Product.ToList());
                var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(30)).SetAbsoluteExpiration(TimeSpan.FromDays(30));
                byte[] AllProductBytes = ConvertData<Product>.ObjectListToByteArray(AllProductList);
                await _cache.SetAsync(CacheKeyConstants.AllProductKey, AllProductBytes, options);
                return ConvertData<Product>.ByteArrayToObjectList(AllProductBytes);
            }


            return ConvertData<Product>.ByteArrayToObjectList(bytesFromCache);
         }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            var bytesFromCache = await _cache.GetAsync(CacheKeyConstants.ProductPrefix + productId);
            if (bytesFromCache.IsNullOrEmpty())
            {
                var productById = await Task.Run(() => _context.Product.Where(product => product.Id == productId).FirstOrDefault());
                if (productById == null)
                {
                    return null;
                }
                var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(30)).SetAbsoluteExpiration(TimeSpan.FromDays(30));
                byte[] ProductByIdBytes = ConvertData<Product>.ObjectToByteArray(productById);
                await _cache.SetAsync(CacheKeyConstants.ProductPrefix + productId, ProductByIdBytes, options);
                return ConvertData<Product>.ByteArrayToObject(ProductByIdBytes);
            }

            return ConvertData<Product>.ByteArrayToObject(bytesFromCache);
        }

        private bool ProductExists(int id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }


}

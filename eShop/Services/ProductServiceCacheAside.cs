using eShop.Data;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.Extensions.Caching.Distributed;
using NuGet.Protocol;
using System.Text;
using System.Text.Json;

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

        public async Task<List<Product>> GetAllProductsAsync()
        {
            //string? productListFromCache = _cache.GetString(CacheKeyConstants.AllProductKey);
            //if (productListFromCache == null)
            //{
            //    if (_context.Product == null) throw new Exception("Entity set 'eShopContext.Product'  is null.");
            //    List<Product> AllProductList = await Task.Run(() => _context.Product.ToList());
            //    var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(30));
            //    _cache.Set(CacheKeyConstants.AllProductKey, ProductListToByteArray(AllProductList), options);
            //    return await Task.Run(() => _context.Product.ToList());
            //}


            return new List<Product>();
        }

        public Task<Product?> GetProductByIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        private List<Product> ByteArrayToProductList(byte[] inputByteArray)
        {
            using MemoryStream ms = new MemoryStream(inputByteArray);

            return JsonSerializer.Deserialize<List<Product>>(ms);
        }

        private byte[] ProductListToByteArray(List<Product> inputProductList)
        {

            Encoding u8 = Encoding.UTF8;
            byte[] result = inputProductList.SelectMany(item => u8.GetBytes(item.ToString())).ToArray();

            return result;


        }
    }

    public static class CacheKeyConstants
    {
        public const string AllProductKey = "eShopAllProducts";
    }
}

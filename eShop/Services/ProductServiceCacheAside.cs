﻿using eShop.Data;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.Text;
using System.Text.Json;
using eShop.Helpers;

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

    }


}

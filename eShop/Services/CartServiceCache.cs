using eShop.Interfaces;
using eShop.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace eShop.Services
{
    public class CartServiceCache : ICartService
    {
        private readonly IDistributedCache _cache;


        public CartServiceCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public Task<Cart> AddItemToCart(string username, int catalogItemId, decimal price, int quantity = 1)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCartAsync(int cartId)
        {
            throw new NotImplementedException();
        }

        public Task<Cart?> GetCartAsync(string cartId)
        {
            throw new NotImplementedException();
        }

        public Task<Cart> SetQuantities(int cartId, Dictionary<string, int> quantities)
        {
            throw new NotImplementedException();
        }

        public Task TransferCartAsync(string anonymousId, string userName)
        {
            throw new NotImplementedException();
        }
    }
}

using eShop.Helpers;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace eShop.Services
{
    public class CartItemServiceCache : ICartItemService
    {
        private readonly IDistributedCache _cache;

        public CartItemServiceCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<List<CartItem>> GetCartItemAsync(int cartId)
        {

            string username = await _cache.GetStringAsync(cartId.ToString());
            if(username == null)
            { 
                return new List<CartItem>();
            }
            byte[] cartItemslistBytes = await _cache.GetAsync(CacheKeyConstants.GetCartItemListKey(username));
            List<CartItem> cartItems = ConvertData<CartItem>.ByteArrayToObjectList(cartItemslistBytes);

            return cartItems;
        }
    }
}

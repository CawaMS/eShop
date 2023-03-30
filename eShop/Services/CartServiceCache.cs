using eShop.Data;
using eShop.Helpers;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace eShop.Services
{
    public class CartServiceCache : ICartService
    {
        private readonly IDistributedCache _cache;
        private int _cartId = 1;


        public CartServiceCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<Cart> AddItemToCart(string username, int itemId, decimal price, int quantity = 1)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(14)).SetAbsoluteExpiration(TimeSpan.FromDays(14));
            byte[] cartItemListBytes = await _cache.GetAsync(CacheKeyConstants.GetCartItemListKey(username));

            if (cartItemListBytes.IsNullOrEmpty()) 
            {
                int cartId = this._cartId++;

                await _cache.SetStringAsync(cartId.ToString(), username, options);
                await _cache.SetStringAsync(username, cartId.ToString(), options);
            }
            else 
            {
                List<CartItem> cartItemList = ConvertData<CartItem>.ByteArrayToObjectList(cartItemListBytes);
                CartItem cartItem = cartItemList.Where(item => item.ItemId == itemId).FirstOrDefault();
                if (cartItem != null)
                {
                    CartItem newCartItem = new CartItem(cartItem.Id, cartItem.Quantity, cartItem.UnitPrice);
                    cartItemList.Remove(cartItem);
                    newCartItem.AddQuantity(1);
                    cartItemList.Add(newCartItem);
                }
                else
                {
                    CartItem newCartItem = new CartItem(itemId, 1, price);
                    cartItemList.Add(newCartItem);
                }

                byte[] CartItemListToUpdateBytes = ConvertData<CartItem>.ObjectListToByteArray(cartItemList);
                await _cache.SetAsync(CacheKeyConstants.GetCartItemListKey(username), CartItemListToUpdateBytes, options);
            }

            return new Cart(username);
        }

        public async Task DeleteCartAsync(int cartId)
        {
            string username = await _cache.GetStringAsync(cartId.ToString());

            if (username == null)
            {
                return;
            }
            await _cache.RemoveAsync(cartId.ToString());
            await _cache.RemoveAsync(username);
            await _cache.RemoveAsync(CacheKeyConstants.GetCartItemListKey(username));
        }

        public async Task<Cart?> GetCartAsync(string username)
        {
            Cart cart = new Cart(username);
            List<CartItem> cartItemList = ConvertData<CartItem>.ByteArrayToObjectList(await _cache.GetAsync(CacheKeyConstants.GetCartItemListKey(username)));
            foreach (var item in cartItemList)
            {
                cart.CopyItem(item);
            }

            return cart;

        }

        public Task<Cart> SetQuantities(int cartId, Dictionary<string, int> quantities)
        {
            throw new NotImplementedException();
        }

        public async Task TransferCartAsync(string anonymousName, string userName)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(14)).SetAbsoluteExpiration(TimeSpan.FromDays(14));

            string anonymousId = await _cache.GetStringAsync(anonymousName);
            byte[] cartItemListBytes = await _cache.GetAsync(CacheKeyConstants.GetCartItemListKey(anonymousName));
            await _cache.SetAsync(CacheKeyConstants.GetCartItemListKey(userName), cartItemListBytes, options);
            await _cache.RemoveAsync(anonymousName);
            await _cache.SetStringAsync(anonymousId, userName);
            await _cache.SetStringAsync(userName, anonymousId);


            throw new NotImplementedException();
        }
    }
}

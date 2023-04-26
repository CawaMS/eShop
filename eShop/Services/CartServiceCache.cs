using eShop.Data;
using eShop.Helpers;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace eShop.Services
{
    public class CartServiceCache : ICartService
    {
        private readonly IDistributedCache _cache;
        private readonly TelemetryClient _telemetryClient;


        public CartServiceCache(IDistributedCache cache, TelemetryClient telemetryClient)
        {
            _cache = cache;
            _telemetryClient = telemetryClient;
        }

        public async Task<Cart> AddItemToCart(string username, int itemId, decimal price, int quantity = 1)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(14)).SetAbsoluteExpiration(TimeSpan.FromDays(14));
            byte[] cartItemListByteArray = await _cache.GetAsync(CacheKeyConstants.GetCartItemListKey(username));
            int _cartId;

            if (cartItemListByteArray.IsNullOrEmpty()) 
            {
                _cartId = await generateCartId();

                await _cache.SetStringAsync(_cartId.ToString(), username, options);
                await _cache.SetStringAsync(username, _cartId.ToString(), options);
                List<CartItem> cartItemList = new List<CartItem>();
                CartItem cartItemToAdd = new CartItem(itemId, quantity, price);
                //cartItemToAdd.SetCartId(_cartId);
                cartItemToAdd.CartId = _cartId;
                cartItemList.Add(cartItemToAdd);
                byte[] newCartItemByteArray = ConvertData<CartItem>.ObjectListToByteArray(cartItemList);
                await _cache.SetAsync(CacheKeyConstants.GetCartItemListKey(username),newCartItemByteArray, options);
            }
            else 
            {
                List<CartItem> cartItemList = ConvertData<CartItem>.ByteArrayToObjectList(cartItemListByteArray);
                CartItem cartItem = cartItemList.Where(item => item.ItemId == itemId).FirstOrDefault();
                if (cartItem != null)
                {
                    CartItem newCartItem = new CartItem(itemId, cartItem.Quantity+1, price);
                    _cartId = cartItem.Id;
                    cartItemList.Remove(cartItem);
                    cartItemList.Add(newCartItem);
                }
                else
                {
                    CartItem newCartItem = new CartItem(itemId, 1, price);
                    string cartIdString = await _cache.GetStringAsync(username);
                    if(cartIdString != null)
                    {
                        int cartId = Int32.Parse(cartIdString);
                        //newCartItem.SetCartId(cartId);
                        newCartItem.CartId = cartId;
                    }

                    _cartId = newCartItem.CartId;

                    cartItemList.Add(newCartItem);
                }

                byte[] CartItemListToUpdateByteArray = ConvertData<CartItem>.ObjectListToByteArray(cartItemList);
                await _cache.SetAsync(CacheKeyConstants.GetCartItemListKey(username), CartItemListToUpdateByteArray, options);
            }

            Cart _cart = new Cart(username);
            //_cart.setId(_cartId);
            _cart.Id = _cartId;
            return _cart;
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
            using (var operation = _telemetryClient.StartOperation<DependencyTelemetry>("AzureCacheForRedis"))
            {
                Cart cart = new Cart(username);
                string cartIdString = await _cache.GetStringAsync(username);
                if (cartIdString == null)
                {
                    return null;
                }

                int cartId = Int32.Parse(cartIdString);
                cart.Id = cartId;

                return cart;
            }


        }

        public async Task<int> GetCartId(Cart cart)
        {
            string username = cart.BuyerId.ToString();

            string cartIdString = await _cache.GetStringAsync(username);

            int cartId = Int32.Parse(cartIdString);

            return cartId;
        }

        public Task<Cart> SetQuantities(int cartId, Dictionary<string, int> quantities)
        {
            throw new NotImplementedException();
        }

        public async Task TransferCartAsync(string anonymousName, string userName)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(14)).SetAbsoluteExpiration(TimeSpan.FromDays(14));

            string anonymousId = await _cache.GetStringAsync(anonymousName);
            if(anonymousId.IsNullOrEmpty())
            { 
                return; 
            }
            
            byte[] cartItemListByteArray = await _cache.GetAsync(CacheKeyConstants.GetCartItemListKey(anonymousName));
            string existingCartId = await _cache.GetStringAsync(userName);
            if (!existingCartId.IsNullOrEmpty())
            {
                await _cache.RemoveAsync(existingCartId);
            }
            if (!cartItemListByteArray.IsNullOrEmpty())
            {
                await _cache.SetAsync(CacheKeyConstants.GetCartItemListKey(userName), cartItemListByteArray, options);
                await _cache.SetStringAsync(anonymousId, userName);
                await _cache.SetStringAsync(userName, anonymousId);

            }

            await _cache.RemoveAsync(anonymousName);
            await _cache.RemoveAsync(CacheKeyConstants.GetCartItemListKey(anonymousName));
        }

        private async Task<int> generateCartId()
        {
            var rand = new Random();
            int cardId = rand.Next();

            string username = "placeholder";

            while (!username.IsNullOrEmpty()) 
            {
                username = await _cache.GetStringAsync(cardId.ToString());
            }

            return cardId;
        }
    }
}

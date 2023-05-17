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

        public async Task AddItem(string username, int itemId, decimal price, int quantity = 1)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2));
            byte[] cartItemListByteArray = await _cache.GetAsync(GetCartItemListKey(username));

            if (cartItemListByteArray.IsNullOrEmpty()) 
            {
                List<CartItem> cartItemList = new List<CartItem>();
                CartItem cartItemToAdd = new CartItem(itemId, quantity, price);
                cartItemList.Add(cartItemToAdd);
                byte[] newCartItemByteArray = await ConvertData<CartItem>.ObjectListToByteArray(cartItemList);
                await _cache.SetAsync(GetCartItemListKey(username),newCartItemByteArray, options);
            }
            else 
            {
                List<CartItem> cartItemList = await ConvertData<CartItem>.ByteArrayToObjectList(cartItemListByteArray).ToListAsync();
                CartItem cartItem = cartItemList.Where(item => item.ItemId == itemId).FirstOrDefault();
                if (cartItem != null)
                {
                    CartItem newCartItem = new CartItem(itemId, cartItem.Quantity+1, price);
                    cartItemList.Remove(cartItem);
                    cartItemList.Add(newCartItem);
                }
                else
                {
                    CartItem newCartItem = new CartItem(itemId, 1, price);

                    cartItemList.Add(newCartItem);
                }

                byte[] CartItemListToUpdateByteArray = await ConvertData<CartItem>.ObjectListToByteArray(cartItemList);
                await _cache.SetAsync(GetCartItemListKey(username), CartItemListToUpdateByteArray, options);
            }
        }

        public async Task DeleteCart(string username)
        {

            if (username == null)
            {
                return;
            }

            await _cache.RemoveAsync(GetCartItemListKey(username));
        }

        public async Task<Cart?> GetCart(string username)
        {

            throw new NotImplementedException();


        }

        public Task<Cart> SetQuantities(int cartId, Dictionary<string, int> quantities)
        {
            throw new NotImplementedException();
        }

        public async Task TransferCart(string anonymousName, string userName)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2));

            
            byte[] cartItemListByteArray = await _cache.GetAsync(GetCartItemListKey(anonymousName));
            if (cartItemListByteArray.IsNullOrEmpty())
            {
                return;
            }

            else 
            {
                await _cache.SetAsync(GetCartItemListKey(userName), cartItemListByteArray, options);
            }

            await _cache.RemoveAsync(GetCartItemListKey(anonymousName));
        }

        public async IAsyncEnumerable<CartItem>? GetCartItems(string username)
        {

            if (username == null)
            {
                yield break;
            }
            byte[] cartItemslist = await _cache.GetAsync(GetCartItemListKey(username));

            if (cartItemslist.IsNullOrEmpty())
            {
                yield break;
            }
            else 
            {
                await foreach (CartItem _cartItem in ConvertData<CartItem>.ByteArrayToObjectList(cartItemslist))
                {
                    yield return _cartItem;
                }
            }


        }

        private async Task<int> generateCartId()
        {
            var rand = new Random();
            int cartId;
            string userName;

            do
            {
                cartId = rand.Next();
                userName = await _cache.GetStringAsync(cartId.ToString());
            } 
            while (!userName.IsNullOrEmpty());

            return cartId;
        }

        private string GetCartItemListKey(string userName)
        {
            return "cartItemList_&_" + userName;
        }

    }
}

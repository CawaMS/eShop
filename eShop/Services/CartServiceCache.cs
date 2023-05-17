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
            //int _cartId;

            if (cartItemListByteArray.IsNullOrEmpty()) 
            {
                //_cartId = await generateCartId();

               // await _cache.SetStringAsync(_cartId.ToString(), username, options);
               // await _cache.SetStringAsync(username, _cartId.ToString(), options);
                List<CartItem> cartItemList = new List<CartItem>();
                CartItem cartItemToAdd = new CartItem(itemId, quantity, price);
                //cartItemToAdd.SetCartId(_cartId);
                //cartItemToAdd.CartId = _cartId;
                //cartItemToAdd.CartId = username;
                cartItemList.Add(cartItemToAdd);
                byte[] newCartItemByteArray = ConvertData<CartItem>.ObjectListToByteArray(cartItemList);
                await _cache.SetAsync(GetCartItemListKey(username),newCartItemByteArray, options);
            }
            else 
            {
                List<CartItem> cartItemList = await ConvertData<CartItem>.ByteArrayToObjectList(cartItemListByteArray).ToListAsync();
                CartItem cartItem = cartItemList.Where(item => item.ItemId == itemId).FirstOrDefault();
                if (cartItem != null)
                {
                    CartItem newCartItem = new CartItem(itemId, cartItem.Quantity+1, price);
                    //_cartId = cartItem.Id;
                    cartItemList.Remove(cartItem);
                    cartItemList.Add(newCartItem);
                }
                else
                {
                    CartItem newCartItem = new CartItem(itemId, 1, price);
                    //string cartIdString = await _cache.GetStringAsync(username);
                    //if(cartIdString != null)
                    //{
                    //    int cartId = Int32.Parse(cartIdString);
                    //    //newCartItem.SetCartId(cartId);
                    //    //newCartItem.CartId = cartId;
                    //    newCartItem.CartId = username;
                    //}

                    //_cartId = newCartItem.CartId;

                    cartItemList.Add(newCartItem);
                }

                byte[] CartItemListToUpdateByteArray = ConvertData<CartItem>.ObjectListToByteArray(cartItemList);
                await _cache.SetAsync(GetCartItemListKey(username), CartItemListToUpdateByteArray, options);
            }
        }

        public async Task DeleteCart(string username)
        {
            //string username = await _cache.GetStringAsync(cartId.ToString());

            if (username == null)
            {
                return;
            }
            //await _cache.RemoveAsync(cartId.ToString());
            //await _cache.RemoveAsync(username);
            await _cache.RemoveAsync(GetCartItemListKey(username));
        }

        public async Task<Cart?> GetCart(string username)
        {
            //using (var operation = _telemetryClient.StartOperation<DependencyTelemetry>("AzureCacheForRedis"))
            //{
            //    Cart cart = new Cart(username);
            //    string cartIdString = await _cache.GetStringAsync(username);
            //    if (cartIdString == null)
            //    {
            //        return null;
            //    }
            //
            //    int cartId = Int32.Parse(cartIdString);
            //    cart.Id = cartId;
            //
            //    return cart;
            //}
            throw new NotImplementedException();


        }

        public Task<Cart> SetQuantities(int cartId, Dictionary<string, int> quantities)
        {
            throw new NotImplementedException();
        }

        public async Task TransferCart(string anonymousName, string userName)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2));

            //string anonymousId = await _cache.GetStringAsync(anonymousName);
            //if(anonymousId.IsNullOrEmpty())
            //{ 
            //    return; 
            //}
            
            byte[] cartItemListByteArray = await _cache.GetAsync(GetCartItemListKey(anonymousName));
            if (cartItemListByteArray.IsNullOrEmpty())
            {
                return;
            }
            //string existingCartId = await _cache.GetStringAsync(userName);
            //if (!existingCartId.IsNullOrEmpty())
            //{
            //    await _cache.RemoveAsync(existingCartId);
            //}
            else 
            {
                await _cache.SetAsync(GetCartItemListKey(userName), cartItemListByteArray, options);
            }

            await _cache.RemoveAsync(GetCartItemListKey(anonymousName));
        }

        public async IAsyncEnumerable<CartItem> GetCartItems(string username)
        {

            //string username = await _cache.GetStringAsync(cartId.ToString());
            if (username == null)
            {
                yield return null;
            }
            byte[] cartItemslist = await _cache.GetAsync(GetCartItemListKey(username));
            await foreach (CartItem _cartItem in ConvertData<CartItem>.ByteArrayToObjectList(cartItemslist))
            {
                yield return _cartItem;
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

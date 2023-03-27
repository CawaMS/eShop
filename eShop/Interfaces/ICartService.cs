using eShop.Models;
using NuGet.ContentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShop.Interfaces;

public interface ICartService
{
    Task TransferCartAsync(string anonymousId, string userName);
    Task<Cart> AddItemToCart(string username, int catalogItemId, decimal price, int quantity = 1);
    Task<Cart> SetQuantities(int basketId, Dictionary<string, int> quantities);
    Task DeleteBasketAsync(int basketId);

}

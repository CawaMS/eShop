using eShop.Data;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.EntityFrameworkCore;

namespace eShop.Services;

public class CartServiceDB : ICartService
{
    private readonly eShopContext _context;

    public CartServiceDB(eShopContext context)
    { 
        _context = context;
    }

    public async Task<Cart> AddItemToCart(string username, int itemId, decimal price, int quantity = 1)
    {
        var cart = _context.carts.Where(cart => cart.BuyerId == username).FirstOrDefault();
        if (cart == null)
        {
            cart = new Cart(username);
            await _context.carts.AddAsync(cart);
        }

        //cart.AddItem(itemId,price,quantity);

        var cartItem = _context.cartItems.Where(item => item.CartId == cart.Id).Where(item => item.ItemId == itemId).FirstOrDefault();
        if (cartItem == null)
        {
            cart.AddItem(itemId, price, quantity);
        }
        else 
        {
            cartItem.AddQuantity(1);
            _context.cartItems.Update(cartItem);
        }
        

        _context.SaveChanges();

        return cart;

    }

    public async Task<Cart?> GetCartAsync(string username)
    {
        var cart = await Task.Run(() => _context.carts.Where(cart => cart.BuyerId == username).FirstOrDefault());

        return cart;
    }

    public Task DeleteCartAsync(int cartId)
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

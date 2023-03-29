using eShop.Data;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;

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

    public async Task DeleteCartAsync(int cartId)
    {
        List<CartItem> cartItemsList = _context.cartItems.Where(item => item.CartId == cartId).ToList();

        if (cartItemsList.Count > 0)
        {
            foreach (var _cartItem in cartItemsList)
            {
                await _context.cartItems.Where(item => item.Id == _cartItem.Id).ExecuteDeleteAsync();
            }
        }

        await _context.carts.Where(_cart => _cart.Id == cartId).ExecuteDeleteAsync();

    }

    public Task<Cart> SetQuantities(int cartId, Dictionary<string, int> quantities)
    {
        throw new NotImplementedException();
    }

    public async Task TransferCartAsync(string anonymousId, string userName)
    {


        var anonymousCart = await _context.carts.Where(cart => cart.BuyerId == anonymousId).FirstOrDefaultAsync();

        if(anonymousCart == null)
        {
            return;
        }

        var userCart = await _context.carts.Where(cart => cart.BuyerId == userName).FirstOrDefaultAsync();

        if (userCart == null)
        {
            userCart = new Cart(userName);
            await _context.carts.AddAsync(userCart);
            _context.SaveChanges();
        }

        List<CartItem> cartItemsList = _context.cartItems.Where(item => item.CartId == anonymousCart.Id).ToList();

        if(cartItemsList.Count > 0) 
        {
            foreach (var _cartItem in cartItemsList)
            {
                await AddItemToCart(userName, _cartItem.ItemId, _cartItem.Quantity);
                await _context.cartItems.Where(item => item.Id == _cartItem.Id).ExecuteDeleteAsync();
            }
        }

        await _context.carts.Where(_cart => _cart.Id == anonymousCart.Id).ExecuteDeleteAsync();

        _context.SaveChanges();


        

    }
}

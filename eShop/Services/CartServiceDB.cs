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

    public async Task<Cart> AddItem(string username, int itemId, decimal price, int quantity = 1)
    {
        var cart = _context.Carts.Where(cart => cart.BuyerId == username).FirstOrDefault();
        if (cart == null)
        {
            cart = new Cart(username);
            await _context.Carts.AddAsync(cart);
        }

        //cart.AddItem(itemId,price,quantity);

        var cartItem = _context.CartItems.Where(item => item.CartId == cart.Id).Where(item => item.ItemId == itemId).FirstOrDefault();
        if (cartItem == null)
        {
            await cart.AddItemAsync(itemId, price, quantity);
        }
        else 
        {
            cartItem.AddQuantity(1);
            _context.CartItems.Update(cartItem);
        }
        

        _context.SaveChanges();

        return cart;

    }

    public async Task<Cart?> GetCart(string username)
    {
        var cart = await Task.Run(() => _context.Carts.Where(cart => cart.BuyerId == username).FirstOrDefault());

        return cart;
    }

    public async Task DeleteCart(int cartId)
    {
        List<CartItem> cartItemsList = _context.CartItems.Where(item => item.CartId == cartId).ToList();

        if (cartItemsList.Count > 0)
        {
            foreach (var _cartItem in cartItemsList)
            {
                await _context.CartItems.Where(item => item.Id == _cartItem.Id).ExecuteDeleteAsync();
            }
        }

        await _context.Carts.Where(_cart => _cart.Id == cartId).ExecuteDeleteAsync();

    }

    public Task<Cart> SetQuantities(int cartId, Dictionary<string, int> quantities)
    {
        throw new NotImplementedException();
    }

    public async Task TransferCart(string anonymousId, string userName)
    {


        var anonymousCart = await _context.Carts.Where(cart => cart.BuyerId == anonymousId).FirstOrDefaultAsync();

        if(anonymousCart == null)
        {
            return;
        }

        var userCart = await _context.Carts.Where(cart => cart.BuyerId == userName).FirstOrDefaultAsync();

        if (userCart == null)
        {
            userCart = new Cart(userName);
            await _context.Carts.AddAsync(userCart);
            _context.SaveChanges();
        }

        List<CartItem> cartItemsList = _context.CartItems.Where(item => item.CartId == anonymousCart.Id).ToList();

        if(cartItemsList.Count > 0) 
        {
            foreach (var _cartItem in cartItemsList)
            {
                await AddItem(userName, _cartItem.ItemId, _cartItem.Quantity);
                await _context.CartItems.Where(item => item.Id == _cartItem.Id).ExecuteDeleteAsync();
            }
        }

        await _context.Carts.Where(_cart => _cart.Id == anonymousCart.Id).ExecuteDeleteAsync();

        _context.SaveChanges();


    }

    public async IAsyncEnumerable<CartItem> GetCartItems(int cartId)
    {
        //return await Task.Run(() => _context.CartItems.Where(item => item.CartId == cartId).ToList());
        foreach (CartItem _cartItem in _context.CartItems.Where(_item => _item.CartId == cartId))
        {
            yield return _cartItem;
        }

    }

}

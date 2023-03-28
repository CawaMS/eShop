using eShop.Data;
using eShop.Interfaces;
using eShop.Models;

namespace eShop.Services;

public class CartItemServiceDB : ICartItemService
{
    private readonly eShopContext _context;

    public CartItemServiceDB(eShopContext context)
    {
        _context=context;
    }

    public async Task<List<CartItem>> GetCartItemAsync(int cartId)
    {
        return await Task.Run(() => _context.cartItems.Where(item => item.CartId == cartId).ToList());

    }
}

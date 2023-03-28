using eShop.Data;
using eShop.Interfaces;
using eShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using System;
using System.Dynamic;

namespace eShop.Views.Carts;

public class IndexModel : PageModel
{
    private readonly ICartService _cartService;
    private readonly IProductService _productService;

    public IndexModel(ICartService cartService, IProductService productService)
    {
        _cartService=cartService;
        _productService=productService;
    }

    public async Task OnGet()
    {
        await Task.Run(() => ViewData["message"]="Hello from shopping cart");
    }

    public async Task<IActionResult> OnPost(Product productDetails)
    {
        if (productDetails?.Id == null)
        {
            return RedirectToPage("/Index");
        }

        var item = await _productService.GetProductByIdAsync(productDetails.Id);
        if (item == null)
        {
            return RedirectToPage("/Index");
        }

        var username = GetOrSetBasketCookieAndUserName();
        var basket = await _cartService.AddItemToCart(username,
            productDetails.Id, item.Price);

        //TODO: get list of all cart items in the current shopping cart

        return RedirectToPage();

    }

    private string GetOrSetBasketCookieAndUserName()
    {
        string? userName = null;

        if (Request.HttpContext.User.Identity.IsAuthenticated)
        {
            return Request.HttpContext.User.Identity.Name!;
        }

        if (Request.Cookies.ContainsKey(Constants.CART_COOKIENAME))
        {
            userName = Request.Cookies[Constants.CART_COOKIENAME];

            if (!Request.HttpContext.User.Identity.IsAuthenticated)
            {
                if (!Guid.TryParse(userName, out var _))
                {
                    userName = null;
                }
            }
        }
        if (userName != null) return userName;

        userName = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions { IsEssential = true };
        cookieOptions.Expires = DateTime.Today.AddYears(10);
        Response.Cookies.Append(Constants.CART_COOKIENAME, userName, cookieOptions);

        return userName;
    }
}

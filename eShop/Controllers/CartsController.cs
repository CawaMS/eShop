using eShop.Interfaces;
using eShop.Models;
using eShop.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace eShop.Controllers
{
    public class CartsController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartsController(ICartService cartService, IProductService productService)
        { 
            _cartService = cartService;
            _productService = productService;
        }
        
        public async Task<ActionResult> Index()
        {

            List<ShoppingCartItem> ShoppingList = new List<ShoppingCartItem>();

            string username = GetOrSetBasketCookieAndUserName();
            List<CartItem> CartItemList = await _cartService.GetCartItems(username).ToListAsync();

            if (CartItemList.IsNullOrEmpty())
            {
                return View(ShoppingList);
            }
            else 
            {
                foreach (var item in CartItemList)
                {
                    var product = await _productService.GetProductByIdAsync(item.ItemId);
                    if (product == null)
                    {
                        return View();
                    }
                    ShoppingList.Add(new ShoppingCartItem { Name=product.Name, Price=product.Price, Quantity=item.Quantity, CartId=username });
                }

                return View(ShoppingList);
            }

        }

        // GET: CartsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }


        // POST: CartsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product productDetails)
        {
            if (productDetails?.Id == null)
            {
                return RedirectToAction("Index","Home");
            }


            var username = GetOrSetBasketCookieAndUserName();
            await _cartService.AddItem(username, productDetails.Id, productDetails.Price);

            return View(productDetails);
        }

        // GET: CartsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CartsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        // POST: CartsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string CartId)
        {
            await _cartService.DeleteCart(CartId);


            return View();
            
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
}

using eShop.Interfaces;
using eShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Controllers
{
    public class CartsController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        // GET: CartsController

        public CartsController(ICartService cartService, IProductService productService)
        { 
            _cartService = cartService;
            _productService = productService;
        }
        
        public ActionResult Index()
        {
            return View();
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
                return RedirectToPage("/Index");
            }

            var item = await _productService.GetProductByIdAsync(productDetails.Id);
            if (item == null)
            {
                return RedirectToPage("/Index");
            }

            var username = GetOrSetBasketCookieAndUserName();
            var cart = await _cartService.AddItemToCart(username,
                productDetails.Id, item.Price);

            //TODO: get list of all cart items in the current shopping cart

            return RedirectToPage("/Index");
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

        // GET: CartsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CartsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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

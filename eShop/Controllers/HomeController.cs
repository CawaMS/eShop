using eShop.Data;
using eShop.Interfaces;
using eShop.Models;
using eShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace eShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;

        public HomeController(ILogger<HomeController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;

        }

        public async Task<IActionResult> IndexAsync()
        {

            List<Product> productList = await _productService.GetAllProductsAsync();



            var _lastViewedId = HttpContext.Session.GetInt32(SessionConstants.LastViewed);

            if (_lastViewedId != null)
            {
                //var _lastViewedProduct = await _productService.GetProductByIdAsync((int)_lastViewedId);
                var _lastViewedProduct = productList.Where(_product => _product.Id == _lastViewedId).FirstOrDefault();
                if (_lastViewedProduct != null)
                {
                    ViewData["lastViewedName"] = _lastViewedProduct.Name;
                    ViewData["lastViewedBrand"] = _lastViewedProduct.Brand;
                    ViewData["_id"]= _lastViewedProduct.Id;
                    ViewData["_name"]= _lastViewedProduct.Name;
                    ViewData["_image"]=_lastViewedProduct.Image;
                    ViewData["_price"]=_lastViewedProduct.Price;
                }
            }

            return View(productList) ;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Details(int id)
        {

            Product _product = await _productService.GetProductByIdAsync(id);

            if (_product == null)
            {
                return NotFound();
            }

            HttpContext.Session.SetInt32(SessionConstants.LastViewed, id);

            return View(_product);
        }
    }
}
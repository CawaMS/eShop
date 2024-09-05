using eShop.Data;
using eShop.Interfaces;
using eShop.Models;
using eShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using StackExchange.Redis;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace eShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public HomeController(ILogger<HomeController> logger, IProductService productService, IConfiguration config)
        {
            _logger = logger;
            _productService = productService;
            _redis = ConnectionMultiplexer.Connect(config["ConnectionStrings:ESHOPREDISCONNECTION"]);
            _db = _redis.GetDatabase();

        }

        public async Task<IActionResult> IndexAsync()
        {

            List<Product> productList = await _productService.GetAllProductsAsync().ToListAsync();
            
            var _lastViewedId = HttpContext.Session.GetInt32(SessionConstants.LastViewed);

            if (_lastViewedId != null)
            {
                var _lastViewedProduct = await _productService.GetProductByIdAsync((int) _lastViewedId);
                if( _lastViewedProduct != null )
                {
                    ViewData["lastViewedName"] = _lastViewedProduct.Name;
                    ViewData["lastViewedBrand"] = _lastViewedProduct.Brand;
                    ViewData["_id"]= _lastViewedProduct.Id;
                    ViewData["_name"]= _lastViewedProduct.Name;
                    ViewData["_image"]=_lastViewedProduct.Image;
                    ViewData["_price"]=_lastViewedProduct.Price;
                }
            }

            //var userOrSessionName = Request.HttpContext.User.Identity.IsAuthenticated? Request.HttpContext.User.Identity.Name : Guid.NewGuid().ToString();
            //var userOrSessionName = "";
            //if (Request.HttpContext.User.Identity.IsAuthenticated)
            //{
            //    userOrSessionName = Request.HttpContext.User.Identity.Name;
            //}
            //else if (Request.Cookies.ContainsKey(Constants.UNIQUE_CACHE_TAG))
            //{
            //    userOrSessionName = Request.Cookies[Constants.UNIQUE_CACHE_TAG];
            //}
            //else 
            //{ 
            //    userOrSessionName = Guid.NewGuid().ToString();
            //    var cookieOptions = new CookieOptions { IsEssential = true };
            //    Response.Cookies.Append(Constants.UNIQUE_CACHE_TAG, userOrSessionName, cookieOptions);
            //}


            //ViewData["userUniqueShoppingKey"] = userOrSessionName;


            return View(productList);
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

        [OutputCache]
        public async Task<IActionResult> Details(int id)
        {
            Product _product = await _productService.GetProductByIdAsync(id);

            if (_product == null)
            {
                return NotFound();
            }

            HttpContext.Session.SetInt32(SessionConstants.LastViewed, id);

            SearchCommands ft = _db.FT();
            var descriptionEmbeddings = _db.HashGet("id:"+id, "description_embeddings");
            // search through the descriptions
            var res1 = ft.Search("vss_products",
                                new Query("*=>[KNN 2 @description_embeddings $query_vec]")
                                .AddParam("query_vec", descriptionEmbeddings)
                                .SetSortBy("__description_embeddings_score")
                                .Dialect(2));

            string _recommendation = "";
            List<Product> _recommendedProducts = new List<Product>();

            foreach (var doc in res1.Documents)
            {
                foreach (var item in doc.GetProperties())
                {
                    if (item.Key == "__description_embeddings_score")
                    {
                        Console.WriteLine($"id: {doc.Id}, score: {item.Value}");
                        Console.WriteLine("Item Name: " + _db.HashGet(doc.Id, "Name"));
                        Console.WriteLine("Item description: " + _db.HashGet(doc.Id, "description"));
                        Console.WriteLine();
                        if(!(doc.Id).Equals("id:"+_product.Id.ToString()))
                        {
                            _recommendation += $"id: {doc.Id}, score: {item.Value} " + " " +
                                                 "Item Name: " + _db.HashGet(doc.Id, "Name") + " "+
                                                "Item description: " + _db.HashGet(doc.Id, "description");

                            _recommendedProducts.Add(await _productService.GetProductByIdAsync(getId(doc.Id)));
                        }
                    }
                }
            }

            ViewData["recommendation"] = _recommendation;
            ViewData["recommendedProudcts"] = _recommendedProducts;


            return View(_product);
        }

        private int getId(string hashId)
        {
            string[] words = hashId.Split(':');
            return Int32.Parse(words[1]);
        }
    }
}
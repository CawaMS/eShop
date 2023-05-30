using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using eShop.Models;
using Microsoft.Extensions.Logging;
namespace eShop.Data;

public class eShopContextSeed
{
    public static async Task SeedAsync(eShopContext eShopContext, ILogger logger, int retry = 0)
    {
        var retryForAvailability = retry;
        try
        {
            if (eShopContext.Database.IsSqlServer())
            {
                eShopContext.Database.Migrate();
            }
            if (!await eShopContext.Product.AnyAsync())
            {
                await eShopContext.Product.AddRangeAsync(GetPreconfiguredProduct());
                await eShopContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            if (retryForAvailability >= 10) throw;

            retryForAvailability++;

            logger.LogError(ex.Message);
            await SeedAsync(eShopContext, logger, retryForAvailability);
            throw;
        }

        static IEnumerable<Product> GetPreconfiguredProduct()
        {
            return new List<Product>
            {
                new Product { Name="Top-handle", Price=77.00M, Brand="CathyDesign", Image="/images/Purses/bag-with-top-handle.png", category="Purse"},
                new Product { Name="Boots", Price=160.00M, Brand="LapinArt",Image="/images/Shoes/boots.jpg", category="Shoes"},
                new Product { Name="Coin", Price=89.00M, Brand="LapinArt",Image="/images/Purses/coin-bag.jpg", category="Purse"},
                new Product { Name="Croc", Price=68.00M, Brand="CathyDesign",Image="/images/Shoes/croc-shoe.jpg", category="Shoes"},
                new Product { Name="Dancing", Price=99.00M, Brand="LapinArt",Image="/images/Shoes/dancing-shoes.jpg", category="Shoes"},
                new Product { Name="Dressing", Price=120.00M, Brand="CathyDesign",Image="/images/Shoes/dressing-shoes.jpg", category="Shoes"},
                new Product { Name="Flat", Price=350.00M, Brand="CathyDesign",Image="/images/Shoes/flat-shoes.jpg", category="Shoes"},
                new Product { Name="Gym", Price=110.00M, Brand="LapinArt",Image="/images/Purses/gym-bag.jpg", category="Purse"},
                new Product { Name="Handle", Price=249.00M, Brand="LapinArt",Image="/images/Purses/handle-bag.jpg", category="Purse"},
                new Product { Name="High heel", Price=210.00M, Brand="CathyDesign",Image="/images/Shoes/high-heel.jpg", category="Shoes"},
                new Product { Name="Loafers", Price=299.00M, Brand="LapinArt",Image="/images/Shoes/loafers.jpg", category="Shoes"},
                new Product { Name="Long boots", Price=235.00M, Brand="CathyDesign",Image="/images/Shoes/long-boots.jpg", category="Shoes"},
                new Product { Name="Luggage", Price=399.00M, Brand="CathyDesign",Image="/images/Purses/lugage-bag.png", category="Purse"},
                new Product { Name="Messenger", Price=229.00M, Brand="LapinArt",Image="/images/Purses/messenger-bag.jpg", category="Purse"},
                new Product { Name="Shoulder bag", Price=267.00M, Brand="CathyDesign",Image="/images/Purses/shoulder-bag.jpg", category="Purse"},
                new Product { Name="Slippers", Price=65.00M, Brand="CathyDesign",Image="/images/Shoes/slippers.jpg", category="Shoes"},
                new Product { Name="sneakers", Price=89.00M, Brand="CathyDesign",Image="/images/Shoes/sneakers.jpg", category="Shoes"},
                new Product { Name="speedy", Price=245.00M, Brand="LapinArt",Image="/images/Purses/speedy-bag.jpg", category="Purse"},
                new Product { Name="Tote", Price=30.00M, Brand="LapinArt",Image="/images/Purses/Tote-bag.png", category="Purse"},
                new Product { Name="Wallet", Price=78.00M, Brand="CathyDesign",Image="/images/Purses/wallet.jpg", category="Purse"},
                new Product { Name="Money", Price=57.00M, Brand="LapinArt",Image="/images/Purses/yellow-coin-bag.jpg", category="Purse"},

            };
        }
    }
}

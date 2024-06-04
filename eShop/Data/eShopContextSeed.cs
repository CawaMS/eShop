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
            if (retryForAvailability >= 1) throw;

            retryForAvailability++;

            logger.LogError(ex.Message);
            await SeedAsync(eShopContext, logger, retryForAvailability);
            throw;
        }

        static IEnumerable<Product> GetPreconfiguredProduct()
        {
            return new List<Product>
            {
                new Product { Name="Top-handle", Price=77.00M, Brand="CathyDesign", Image="/images/Purses/bag-with-top-handle.png", category="Purse", description="A purse with top handle. Multiple colors avaiable. Suitable for occasions such as going to the office, weekends hang-outs, going out for dinners, and parties."},
                new Product { Name="Boots", Price=160.00M, Brand="LapinArt",Image="/images/Shoes/boots.jpg", category="Shoes", description="Vegan-leather boots. Multiple colors available. Suitable to wear in spring and autumn. Suitable to both formal and casual occasions."},
                new Product { Name="Coin", Price=89.00M, Brand="LapinArt",Image="/images/Purses/coin-bag.jpg", category="Purse", description="Made of canvas. A small money bag or pouch, made for carrying coins. Matches well with a purse like Speedy, Top-handle, or Messenger"},
                new Product { Name="Croc", Price=68.00M, Brand="CathyDesign",Image="/images/Shoes/croc-shoe.jpg", category="Shoes", description="All Crocs shoes are uniquely designed and manufactured using the company's proprietary closed-cell resin, Croslite, a technology that gives each pair of shoes the soft, comfortable, lightweight, non-marking and odor-resistant qualities that Crocs wearers know and love."},
                new Product { Name="Dancing", Price=99.00M, Brand="LapinArt",Image="/images/Shoes/dancing-shoes.jpg", category="Shoes", description = "Ballet shoes are worn by dancers of all ages, boys and girls, for ballet technique class, and by younger students for performances. Ballet shoes are very lightweight and made of leather or canvas."},
                new Product { Name="Dressing", Price=120.00M, Brand="CathyDesign",Image="/images/Shoes/dressing-shoes.jpg", category="Shoes", description="Vegan-leather dressing shoes. Only one color available. Suitable to wear all seasons. Suitable to formal occasions."},
                new Product { Name="Flat", Price=350.00M, Brand="CathyDesign",Image="/images/Shoes/flat-shoes.jpg", category="Shoes", description="Minimalist and monochrome or adorned (with tassels, laces, stones), they are very lightweight to wear and allow the foot to move with great ease."},
                new Product { Name="Gym", Price=110.00M, Brand="LapinArt",Image="/images/Purses/gym-bag.jpg", category="Purse", description="Gym bads are made with enough space to handle all your gear and built sturdy for easy transport. Stash your essentials and everything else you need with ease in roomy duffle bags. Gym bags are made with enough space to handle all your gear and built sturdy for easy transport."},
                new Product { Name="Handle", Price=249.00M, Brand="LapinArt",Image="/images/Purses/handle-bag.jpg", category="Purse", description="A purse with handle. Only one color available. Suitable for traveling in all seasons."},
                new Product { Name="High heel", Price=210.00M, Brand="CathyDesign",Image="/images/Shoes/high-heel.jpg", category="Shoes", description="High-heeled shoes, also known as high heels or pumps, are a type of shoe with an upward-angled sole. The heel in such shoes is raised above the ball of the foot. High heels cause the legs to appear longer, make the wearer appear taller, and accentuate the calf muscle."},
                new Product { Name="Loafers", Price=299.00M, Brand="LapinArt",Image="/images/Shoes/loafers.jpg", category="Shoes", description=" a slip-on shoe that was popular with men and women as a casual option. The design was simple, with no laces or buckles, and a low, flat heel. The upper part of the shoe was typically made from soft leather or suede, and the sole was often made of rubber."},
                new Product { Name="Long boots", Price=235.00M, Brand="CathyDesign",Image="/images/Shoes/long-boots.jpg", category="Shoes", description="Vegan-leather long boots. Multiple colors available. Suitable to wear in autumn and winter. Suitable for formal occasions."},
                new Product { Name="Luggage", Price=399.00M, Brand="CathyDesign",Image="/images/Purses/lugage-bag.png", category="Purse", description="suitcases or other bags in which to pack personal belongings for travel; something lugged; bags and suitcases that contain possessions you take with you when traveling"},
                new Product { Name="Messenger", Price=229.00M, Brand="LapinArt",Image="/images/Purses/messenger-bag.jpg", category="Purse", description="A purse with cross-body straps. Multiple colors available. Suitable for casual occasions."},
                new Product { Name="Shoulder bag", Price=267.00M, Brand="CathyDesign",Image="/images/Purses/shoulder-bag.jpg", category="Purse", description="a bag that hangs on a strap from the shoulder, especially one used for carrying small personal things"},
                new Product { Name="Slippers", Price=65.00M, Brand="CathyDesign",Image="/images/Shoes/slippers.jpg", category="Shoes", description="easy to put on and off and are intended to be worn indoors, particularly at home. They provide comfort and protection for the feet when walking indoors."},
                new Product { Name="sneakers", Price=89.00M, Brand="CathyDesign",Image="/images/Shoes/sneakers.jpg", category="Shoes", description="Sneakers are made for exercise and sports, but they're also very popular everyday shoes because they're so comfortable. Sneaker, which is most common in the Northeast US, comes from their noiseless rubber soles, perfect for sneaking. Originally, they were called sneaks."},
                new Product { Name="speedy", Price=245.00M, Brand="LapinArt",Image="/images/Purses/speedy-bag.jpg", category="Purse", description="A purse with top handle and cross-body straps. Only one color available. Suitable for occasions such as going to the office, weekends hang-outs, shopping, and parties."},
                new Product { Name="Tote", Price=30.00M, Brand="LapinArt",Image="/images/Purses/Tote-bag.png", category="Purse", description="a large, typically unfastened bag with parallel handles that emerge from the sides of its pouch. Totes are often used as reusable shopping bags. The archetypal tote bag is made of sturdy cloth, perhaps with thick leather at its handles or bottom; leather versions often have a pebbled surface."},
                new Product { Name="Wallet", Price=78.00M, Brand="CathyDesign",Image="/images/Purses/wallet.jpg", category="Purse", description="a flat case or pouch, often used to carry small personal items such as physical currency, debit cards, and credit cards; identification documents such as driving licence, identification card, club card; photographs, transit pass, business cards and other paper or laminated cards."},
                new Product { Name="Money", Price=57.00M, Brand="LapinArt",Image="/images/Purses/yellow-coin-bag.jpg", category="Purse", description="A small money bag that is yellow in color, made for carrying coins. Matches well with a purse like Speedy, Top-handle, or Messenger"},

            };
        }
    }
}
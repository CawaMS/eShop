using Azure.Identity;
using eShop.Data;
using eShop.Interfaces;
using eShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

#pragma warning disable EXTEXP0018 // pragma warning for HybridCache

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddLogging();

builder.Services.AddDbContext<eShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("eShopContext") ?? throw new InvalidOperationException("Connection string 'eShopContext' not found.")));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddMvc();

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.AddAuthorization();

//Adding for Entra ID authentication of Redis Cache
var configurationOptions = await ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("eShopRedisConnection") ?? throw new InvalidOperationException("Could not find a 'eShopRedisConnection' connection string.")).ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());


//Adding Redis Provider for IDistributedCache
builder.Services.AddStackExchangeRedisCache(options =>
{
    // options.Configuration = builder.Configuration.GetConnectionString("eShopRedisConnection");
    options.ConnectionMultiplexerFactory = async () => await ConnectionMultiplexer.ConnectAsync(configurationOptions);
    options.InstanceName = "eShopCache";
});

builder.Services.AddHybridCache();

//Adding Session Provider, which automatically detects the configured IDistributedCache provider as the session store. In this case, Redis Cache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(14);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    // options.Configuration = builder.Configuration.GetConnectionString("eShopRedisConnection");
    options.ConnectionMultiplexerFactory = async () => await ConnectionMultiplexer.ConnectAsync(configurationOptions);
    options.InstanceName = "eShopOutputCache";
});

builder.Services.AddSingleton<ConnectionMultiplexer>(ConnectionMultiplexer.Connect(configurationOptions));

//Using Redis Cache to implement services for optimizing data services performance
builder.Services.AddScoped<ICartService,CartServiceCache>();

//Using Redis Cache to implement services for optimizing data services performance
builder.Services.AddScoped<IProductService,ProductServiceCacheAside>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<eShopContext>();
    await eShopContextSeed.SeedAsync(context, app.Logger);
    await DescriptionEmbeddings.GenerateEmbeddingsInRedis(context, app.Logger, app.Configuration);
    // await DescriptionEmbeddings.GenerateEmbeddingsInRedis(context, app.Logger, builder.Configuration);
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    var testUserPw = builder.Configuration.GetValue<string>("SeedUserPW");

    await SeedData.Initialize(services, "Admin@12345");
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseAuthorization();

//using session service
app.UseSession();

app.UseOutputCache();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

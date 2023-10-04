using System.Reflection;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pim.CommonLib;
using PimWeb.AppCode;

var Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var appRootPath = builder.Environment.ContentRootPath;

var log4NetConfigFilePath = Path.Combine(appRootPath, "log4net.config");
var logConfigFileInfo = new FileInfo(log4NetConfigFilePath);
if (logConfigFileInfo.Exists)
    XmlConfigurator.ConfigureAndWatch(logConfigFileInfo);
else
    XmlConfigurator.Configure();

//var appOptions = builder.Configuration.Get<AppOptions>();

var appOptions = builder.Configuration.GetSection(nameof(AppOptions)).Get<AppOptions>();

builder.Services
    .AddSingleton(appOptions)
    .AddControllers();
    //use controllers for now
    //.AddRazorPages();

builder.Services.AddMvc(o => o.EnableEndpointRouting = false); //

builder.Services
    .AddLogging()
    .AddDbContext<DatabaseContext>()
    .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString))
    .AddDatabaseDeveloperPageExceptionFilter()
    .AddIdentity<IdentityUser<int>, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password = new PasswordOptions
        {
            RequireDigit = false
            , RequireLowercase = false
            , RequireUppercase = false
            , RequiredUniqueChars = 4
            , RequiredLength = 6
            , RequireNonAlphanumeric = false
        };
    })
    //.AddDefaultIdentity<IdentityUser<int>>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole<int>>();

builder.Services.AddAuthorization();

//builder.Services.AddAuthorization(options =>
//{
//    options.FallbackPolicy = new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .Build();
//});

// to customize auth
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/Login";
    o.SlidingExpiration = true;
    o.ExpireTimeSpan = TimeSpan.FromDays(1);
    o.Cookie.Name = ".auth";
});

builder.Logging.ClearProviders().AddLog4Net();

var app = builder.Build();

var ifSeed = "/seed".EqualsIgnoreCase(args.FirstOrDefault());
if (ifSeed)
{
    Log.Info("Seeding identity");
    using var scope = app.Services.CreateScope();
    
    var seedUsers = builder.Configuration.GetSection(nameof(SeedUsers)).Get<SeedUsers>();
    if (seedUsers?.Users?.Count > 0)
        await scope.ServiceProvider.Seed(seedUsers);
    else
        Log.Warn("No data to seed found in the configuration.");
    
    return; 
}
    
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapDefaultControllerRoute();

app
    .UseHttpsRedirection()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    // must come after UseAuthentication
    .UseMvc();

app.MapControllers();

//app.MapRazorPages();

app.Run();

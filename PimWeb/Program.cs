using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FulltextStorageLib;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pim.CommonLib;
using PimIdentity;
using PimWeb.AppCode;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.DependencyInjection;
using Raven.Identity;
using IdentityConstants = PimWeb.AppCode.IdentityConstants;

var Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

var builder = WebApplication.CreateBuilder(args);
var appRootPath = builder.Environment.ContentRootPath;

var log4NetConfigFilePath = Path.Combine(appRootPath, "log4net.config");
var logConfigFileInfo = new FileInfo(log4NetConfigFilePath);
if (logConfigFileInfo.Exists)
    XmlConfigurator.ConfigureAndWatch(logConfigFileInfo);
else
    XmlConfigurator.Configure();

//var appOptions = builder.Configuration.Get<AppOptions>();

var appOptions = builder.Configuration.GetSection(nameof(AppOptions)).Get<AppOptions>();

var appDataPath = appOptions.DataPath.IsNullOrEmpty()
    ? Path.Combine(appRootPath, "App_Data")
    : appOptions.DataPath;

Console.WriteLine("Using data directory '{0}'", appDataPath);

Directory.CreateDirectory(appDataPath);

var dbPath = Path.Combine(appDataPath, "Pim.db");
var database = NoteLiteDb.GetNoteDatabase($"Filename={dbPath}; Upgrade=true; Initial Size=5MB; Password=;");

// var authDatabase = database;
// var identityDatabaseContextFactory = new IdentityDatabaseContextFactory(authDatabase);
// var identityConfiguration = new IdentityConfiguration(identityDatabaseContextFactory);
//
// var authSeedTask = identityConfiguration.EnsureDefaultUsersAndRolesAsync();

var storage = NoteStorage.CreateStandard(database, appDataPath, true);
storage.Open();

var languageSetting = appOptions.FulltextIndexLanguages?.WhereNotWhiteSpace().ToArray();
if (!(languageSetting?.Length > 0))
    languageSetting = new [] { "English", "Russian"};

var stemmerNames = languageSetting.WhereNotWhiteSpace().ToCaseInsensitiveSet();
var redundantIndexes = storage.ActiveIndexNames.Where(name => !stemmerNames.Contains(name));
foreach (var redundantIndexName in redundantIndexes)
{
    Log.InfoFormat("Removing FT index {0}", redundantIndexName);
    storage.RemoveIndex(redundantIndexName);
}

stemmerNames.ExceptWith(storage.ActiveIndexNames);

storage.OpenOrCreateIndexes(stemmerNames);

storage.MultiIndex.UseFuzzySearch = true;

//builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Register services directly with Autofac here.
// Don't call builder.Populate(), that happens in AutofacServiceProviderFactory.
/*
 autofac has trouble resolving RavenDB UserStore constructor
builder.Host.ConfigureContainer<ContainerBuilder>(
    conBuilder =>
    {
        conBuilder.Register<INoteStorage>(context => storage).SingleInstance();
        //conBuilder.Register(c => identityDatabaseContextFactory).SingleInstance();
        //conBuilder.Register(c => identityDatabaseContextFactory.Create()).InstancePerLifetimeScope();
    });
*/

builder.Services
//    .AddAutofac()
    .AddSingleton(appOptions)
    .AddControllers();
    //use controllers for now
    //.AddRazorPages();

//builder.Services.AddSingleton<ILiteDbContext>(identityDatabaseContextFactory.DbContext);

builder.Services.AddMvc(o => o.EnableEndpointRouting = false); //

builder.Services
    .AddLogging()
    .AddRavenDbDocStore()
    .AddRavenDbAsyncSession()
    .AddIdentity<Raven.Identity.IdentityUser, Raven.Identity.IdentityRole>(ApplicationUserManager.SetOptions)
    // use appsettings.json config
    .AddRavenDbIdentityStores<Raven.Identity.IdentityUser, Raven.Identity.IdentityRole>()
    .AddRoles<Raven.Identity.IdentityRole>()
    .AddDefaultTokenProviders();

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

using (var scope = app.Services.CreateScope())
{
    var seedUsers = builder.Configuration.GetSection(nameof(SeedUsers)).Get<SeedUsers>();
    if (seedUsers?.Users?.Count > 0)
    {
        await scope.ServiceProvider.Seed(seedUsers);
        var databaseSession = scope.ServiceProvider.GetRequiredService<IAsyncDocumentSession>();
        
        // RavenDB.Identity inconsistently does not commit changes after adding roles to users (probably a bug)
        await databaseSession.SaveChangesAsync();
    }
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
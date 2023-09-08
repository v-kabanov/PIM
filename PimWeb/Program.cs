using System.Reflection;
using AspNetCore.Identity.LiteDB;
using AspNetCore.Identity.LiteDB.Data;
using AspNetCore.Identity.LiteDB.Models;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FulltextStorageLib;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Identity;
using Pim.CommonLib;
using PimIdentity;
using IdentityRole = AspNetCore.Identity.LiteDB.IdentityRole;

var Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

var builder = WebApplication.CreateBuilder(args);
var appRootPath = builder.Environment.ContentRootPath;

var log4NetConfigFilePath = Path.Combine(appRootPath, "log4net.config");
var logConfigFileInfo = new FileInfo(log4NetConfigFilePath);
if (logConfigFileInfo.Exists)
    XmlConfigurator.ConfigureAndWatch(logConfigFileInfo);
else
    XmlConfigurator.Configure();

var appOptions = new AppOptions();

var appOptionsSection = builder.Configuration.GetSection(nameof(AppOptions));
appOptionsSection.Bind(appOptions);

var appDataPath = appOptions.DataPath.IsNullOrEmpty()
    ? Path.Combine(appRootPath, "App_Data")
    : appOptions.DataPath;

Directory.CreateDirectory(appDataPath);

var dbPath = Path.Combine(appDataPath, "Pim.db");
var database = NoteLiteDb.GetNoteDatabase($"Filename={dbPath}; Upgrade=true; Initial Size=5MB; Password=;");

var authDatabase = database;
var identityDatabaseContextFactory = new IdentityDatabaseContextFactory(authDatabase);
var identityConfiguration = new IdentityConfiguration(identityDatabaseContextFactory);

var authSeedTask = identityConfiguration.EnsureDefaultUsersAndRolesAsync();

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

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Register services directly with Autofac here.
// Don't call builder.Populate(), that happens in AutofacServiceProviderFactory.
builder.Host.ConfigureContainer<ContainerBuilder>(
    conBuilder =>
    {
        conBuilder.Register(context => storage).SingleInstance();
        conBuilder.Register(c => identityDatabaseContextFactory).SingleInstance();
        conBuilder.Register(c => identityDatabaseContextFactory.Create()).InstancePerLifetimeScope();
    });

builder.Services
    .AddAutofac()
    .AddSingleton(appOptions)
    .AddRazorPages();

builder.Services.AddSingleton<ILiteDbContext>(identityDatabaseContextFactory.DbContext);

builder.Services
    .AddLogging()
    .AddScoped<UserManager<ApplicationUser>>(serviceProvider => ApplicationUserManager.Create(serviceProvider))
    .AddScoped<RoleManager<IdentityRole>>(serviceProvider => ApplicationRoleManager.Create(serviceProvider))
    .AddIdentity<ApplicationUser, IdentityRole>(options => ApplicationUserManager.SetOptions(options))
    .AddUserStore<LiteDbUserStore<ApplicationUser>>()
    .AddRoleStore<LiteDbRoleStore<IdentityRole>>()
    //.AddUserManager<ApplicationUserManager>()
    //.AddRoleManager<ApplicationRoleManager>()
    .AddDefaultTokenProviders();

// to customize auth
//builder.Services.ConfigureApplicationCookie(o =>
//{
//    o.LoginPath = "Account/Login";
//    o.SlidingExpiration = true;
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app
    .UseHttpsRedirection()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization();

app.MapRazorPages();

authSeedTask.GetAwaiter().GetResult();

app.Run();
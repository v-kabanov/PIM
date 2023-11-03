using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NHibernate.AspNetCore.Identity;
using NHibernate.NetCore;
//using Microsoft.EntityFrameworkCore;
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

builder.Services.AddMvc(o => o.EnableEndpointRouting = false);

var rawConfig = new NHibernate.Cfg.Configuration();

rawConfig.SetNamingStrategy(new PostgreSqlNamingStrategy());

var configuration = Fluently.Configure(rawConfig)
    .Database(PostgreSQLConfiguration.Standard.ConnectionString(connectionString).ShowSql);
                
var sessionFactory = configuration.BuildSessionFactory();

builder.Services
    .AddSingleton(sessionFactory)
    .AddScoped(serviceProvider =>
    {
        var session = sessionFactory.OpenSession();
        session.BeginTransaction();
        return session;
    })
    .AddHttpLogging(o =>
    {
        o.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.RequestQuery;
    })
    .AddLogging()
    //.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(connectionString))
    //.AddDbContext<DatabaseContext>(o => o.UseNpgsql(connectionString))
    .AddDatabaseDeveloperPageExceptionFilter()
    .AddIdentity<Microsoft.AspNetCore.Identity.IdentityUser<int>, Microsoft.AspNetCore.Identity.IdentityRole<int>>(options =>
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
    //.AddNHibernateStores(t => t.SetSessionAutoFlush(false))
    .AddHibernateStores()
    .AddDefaultTokenProviders()
    .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole<int>>();

builder.Services.AddHibernate();

builder.Services.AddAuthorization();

var virtualPathBase = appOptions.WebAppPath.IsNullOrWhiteSpace()
    ? Environment.GetEnvironmentVariable("APP_VIRTUAL_PATH")
    : appOptions.WebAppPath;

Console.WriteLine("Using base virtual path '{0}'", virtualPathBase);
if (!virtualPathBase.IsNullOrWhiteSpace())
{
    virtualPathBase = virtualPathBase.Trim().Trim('/');
    if (!virtualPathBase.IsNullOrEmpty())
    {
        virtualPathBase = '/' + virtualPathBase;
    }
}

if (virtualPathBase.IsNullOrEmpty())
    virtualPathBase = "/";

Log.InfoFormat("Using base virtual path '{0}'", virtualPathBase);

builder.Services.ConfigureApplicationCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/Login";
        o.LogoutPath = "/Account/LogOff";
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromDays(30);
        o.Cookie.Name = ".auth";
    });

builder.Services.AddScoped<INoteService, NoteService>();

builder.Logging.ClearProviders().AddLog4Net();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseHttpLogging();

using var session = sessionFactory.OpenSession();

var ifSeed = "/seed".EqualsIgnoreCase(args.FirstOrDefault()) || !session.Query<IdentityUser<int>>().Any();
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

// make it possible to run under reverse proxy  //XForwardedFor | ForwardedHeaders.XForwardedProto
app.UsePathBase(virtualPathBase)
    .UseRouting()
    .UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
    //app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
//}

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapDefaultControllerRoute();

app
    //.UseHttpsRedirection()
    .UseStaticFiles()
    .UseAuthentication()
    .UseAuthorization()
    // must come after UseAuthentication
    .UseMvc();

app.MapControllers();

//app.MapRazorPages();

app.Run();

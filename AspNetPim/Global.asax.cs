using Autofac;
using Autofac.Integration.Mvc;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AspNetPim.Controllers;
using AspNetPim.Models;
using Autofac.Core;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using FulltextStorageLib;
using FulltextStorageLib.Util;
using LiteDB;
using PimIdentity.Models;

namespace AspNetPim
{
    public class MvcApplication : HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string AppSettingKeyFulltextIndexLanguages = "FulltextIndexLanguages";

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ConfigureBackend();
        }

        public static INoteStorage Storage { get; private set; }

        public static LiteDatabase AuthDatabase { get; private set; }

        private void ConfigureBackend()
        {
            var appDataPath = Server.MapPath("~/App_Data");

            var dbPath = Path.Combine(appDataPath, "Pim.db");
            var database = NoteLiteDb.GetNoteDatabase($"Filename={dbPath}; Upgrade=true; Initial Size=5MB; Password=;");

            AuthDatabase = database;

            var storage = NoteStorage.CreateStandard(database, appDataPath, true);
            storage.Open();

            var languageSetting = ConfigurationManager.AppSettings[AppSettingKeyFulltextIndexLanguages];
            if (string.IsNullOrWhiteSpace(languageSetting))
                languageSetting = "English,Russian";

            var stemmerNames = languageSetting.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries).ToCaseInsensitiveSet();
            var redundantIndexes = storage.ActiveIndexNames.Where(name => !stemmerNames.Contains(name));
            foreach (var redundantIndexName in redundantIndexes)
            {
                Log.InfoFormat("Removing FT index {0}", redundantIndexName);
                storage.RemoveIndex(redundantIndexName);
            }

            stemmerNames.ExceptWith(storage.ActiveIndexNames);

            storage.OpenOrCreateIndexes(stemmerNames);

            storage.MultiIndex.UseFuzzySearch = true;

            Storage = storage;

            var builder = new ContainerBuilder();

            builder.Register(context => Storage).SingleInstance();
            builder.RegisterType<HomeController>();
            builder.RegisterType<ViewEditController>();
            builder.RegisterType<SearchController>();
            builder.Register(c => new IdentityDatabaseContext(AuthDatabase)).SingleInstance();
            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            IServiceLocator autofacServiceLocator = new AutofacServiceLocator(container) as IServiceLocator;
            ServiceLocator.SetLocatorProvider(() => autofacServiceLocator);
        }

        protected void Application_End()
        {
            Storage?.Dispose();
            AuthDatabase?.Dispose();
        }
    }
}

// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-31
// Comment  
// **********************************************************************************************/

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AspNetPim.Controllers;
using Autofac;
using Autofac.Integration.Mvc;
using FulltextStorageLib;
using log4net;
using LiteDB;
using Pim.CommonLib;
using PimIdentity;

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

            var stemmerNames = languageSetting.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToCaseInsensitiveSet();
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
            builder.Register(c => new IdentityDatabaseContextFactory(AuthDatabase)).SingleInstance();
            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        protected void Application_End()
        {
            Storage?.Dispose();
            AuthDatabase?.Dispose();
        }
    }
}
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
using AspNetPim.Models;
using FulltextStorageLib;
using FulltextStorageLib.Util;

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

        private void ConfigureBackend()
        {
            var appDataPath = Server.MapPath("~/App_Data");
            var storageRootPath = Path.Combine(appDataPath, "UserData");

            var storage = NoteStorage.CreateStandard(storageRootPath, true);
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
            builder.RegisterType<HomeViewModel>();

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        protected void Application_End()
        {
            Storage?.Dispose();
        }
    }
}

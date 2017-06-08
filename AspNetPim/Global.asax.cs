using Autofac;
using Autofac.Integration.Mvc;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FulltextStorageLib;

namespace AspNetPim
{
    public class MvcApplication : HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string AppSettingKeyFulltextIndexLanguages = "FulltextIndexLanguages";

        private ISet<string> _newIndexNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

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

            Storage = NoteStorage.CreateStandard(storageRootPath);

            var builder = new ContainerBuilder();

            builder.Register(context => Storage).SingleInstance();

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}

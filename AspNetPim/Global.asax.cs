using AuNoteLib;
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
        }

        public SearchEngine<INote, INoteHeader> SearchEngine { get; private set; }

        private void ConfigureBackend()
        {
            var appDataPath = Server.MapPath("~/App_Data");
            var storageRootPath = Path.Combine(appDataPath, "UserData");

            var fullTextFolder = Path.Combine(storageRootPath, "ft");
            var dbFolder = Path.Combine(storageRootPath, "db");

            var storage = new NoteStorage(dbFolder);
            var adapter = new LuceneNoteAdapter();
            SearchEngine = new SearchEngine<INote, INoteHeader>(fullTextFolder, adapter, new MultiIndex(adapter.DocumentKeyName));

            var builder = new ContainerBuilder();

            builder.Register<INoteStorage>(context => storage).SingleInstance();
            builder.Register(context => SearchEngine).SingleInstance();

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}

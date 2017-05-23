using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AspNetPim.Startup))]
namespace AspNetPim
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

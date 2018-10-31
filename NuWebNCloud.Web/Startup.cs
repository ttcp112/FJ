using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NuWebNCloud.Web.Startup))]
namespace NuWebNCloud.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

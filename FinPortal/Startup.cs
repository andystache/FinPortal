using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FinPortal.Startup))]
namespace FinPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

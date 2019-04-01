using ImportPermitPortal;
using Microsoft.Owin;
using Owin;
using PetroleumProductImportPermit;

[assembly: OwinStartupAttribute(typeof(Startup))]
namespace PetroleumProductImportPermit
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

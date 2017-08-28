using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(API.Startup))]
namespace API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

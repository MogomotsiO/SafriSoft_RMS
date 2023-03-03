using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SafriSoftv1._3.Startup))]
namespace SafriSoftv1._3
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

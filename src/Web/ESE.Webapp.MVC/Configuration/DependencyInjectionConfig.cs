using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ESE.Webapp.MVC.Extensions;
using ESE.Webapp.MVC.Services;

namespace ESE.Webapp.MVC.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddHttpClient<IAutenticacaoService, AutenticacaoService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUser, AspNetUser>();
        }
    }
}
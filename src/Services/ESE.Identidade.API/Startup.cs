using System;
using System.Text;
using ESE.Identidade.API.Data;
using ESE.Identidade.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


namespace ESE.Identidade.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
           services.AddDbContext<ApplicationDbContext>(optionsAction: options =>
            options.UseMySql(Configuration.GetConnectionString("IdentityConnection")));

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // JWT

            var appSettingsSection = Configuration.GetSection(key: "AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(configureOptions: options => 
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions => {
                bearerOptions.RequireHttpsMetadata = true;
                bearerOptions.SaveToken = true;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = appSettings.ValidoEm,
                    ValidIssuer = appSettings.Emissor
                };
            });

            services.AddControllers();

            services.AddSwaggerGen(c => {
                c.SwaggerDoc(name: "v1", new OpenApiInfo
                {
                    Title = "EcomStore Enterprise Identity API",
                    Description = "API responsável pela autenticação de usuário",
                    Contact = new OpenApiContact() {Name = "Roberto Timóteo da Silva", Email = "betinhoorl@gmail.com" },
                    License = new OpenApiLicense() {Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT")}
                });

            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name:"v1");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

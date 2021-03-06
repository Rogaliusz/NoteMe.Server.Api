﻿using System;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NoteMe.Server.Api.Middlewares;
using NoteMe.Server.Infrastructure.Framework.Generators;
using NoteMe.Server.Infrastructure.Framework.Security;
using NoteMe.Server.Infrastructure.IoC;

namespace NoteMe.Server.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public ILifetimeScope Container { get; private set; }
        public SecuritySettings SecuritySettings { get; private set; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddTransient<ExceptionMiddleware>();
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    if (SecuritySettings == null)
                    {
                        SecuritySettings = Container.Resolve<SecuritySettings>();
                    }

                    var key = Encoding.ASCII.GetBytes(SecuritySettings.Key);

                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
        }
        
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new MainModule(Configuration));
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Container = app.ApplicationServices.GetAutofacRoot();
            
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            SeedDataAsync().Wait();
        }

        private Task SeedDataAsync()
        {
            var seeder = Container.Resolve<IDataSeeder>();
            return seeder.SeedAsync();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskAuthenticationAuthorization.Models;

namespace TaskAuthenticationAuthorization
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ShoppingContext>(options => options.UseSqlServer(connection));
            services.AddControllersWithViews();

            // Add authentication and configure cookie settings
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Accounts/Login"; // Redirect to login if unauthorized
                    options.LogoutPath = "/Accounts/Logout";
                    options.AccessDeniedPath = "/Accounts/AccessDenied"; // Redirect if access is denied
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Customize cookie expiration
                    options.SlidingExpiration = true; // Refresh cookie expiration on each request

                });
            services.AddAuthorization(options =>
            {
                // Policy requiring Admin role
                options.AddPolicy("AdminOnly", policy =>
                {
                        policy.RequireRole(UserRole.Admin.ToString());
                    });

                options.AddPolicy("BuyerOnly", policy =>
           {
                        policy.RequireRole(UserRole.Buyer.ToString());
                    });

                options.AddPolicy("PremiumBuyerOnly", policy =>
            {
                        policy.RequireClaim("BuyerType", BuyerType.Golden.ToString(), BuyerType.Wholesale.ToString());
                    });

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Enable authentication and authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

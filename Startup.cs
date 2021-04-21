using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Synonyms.Data;

namespace Synonyms
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.

        //place where you do the registration of service (any functionality that you want to register, email, database
        public void ConfigureServices(IServiceCollection services)
        {
            //to get sql server usable
            services.AddDbContext<SynonymDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection")
                ));

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //this is a middleware
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //all these are middlewares
            app.UseHttpsRedirection();

            //static files load here, so that's important i guess
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            //endpoints are picked up and processed (routed) by the middlwares to the server
            app.UseEndpoints(endpoints =>
            {
                //routing for the url's
                //the question mark behind id makes the id optional in any given url pattern
                //there are mvc and razor pages use the same url pattern
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

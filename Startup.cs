using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMVC.ExtendMethods;
using AppMVC.Models;
using AppMVC.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AppMVC
{
    public class Startup
    {
        public static string ContentRootPath { get; set; }
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            ContentRootPath = env.ContentRootPath;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectString = Configuration.GetConnectionString("AppMvcConnectionString");
                options.UseSqlServer(connectString);
            });

            services.AddControllersWithViews();
            services.AddRazorPages(); // To use Razor Pages

            // 2.2 Custom View folder: if not found in View, continue find in MyView
            services.Configure<RazorViewEngineOptions>(options =>
            {
                // Default: /View/Controller/Action.cshtml
                // Custom: /MyView/Controller/Action.cshtml
                // {0} -> Action
                // {1} -> Controller
                // {2} -> Area
                options.ViewLocationFormats.Add("/MyView/{1}/{0}" + RazorViewEngine.ViewExtension);
            });

            // 3.3 Register Service
            // services.AddSingleton<ProductService>();
            // services.AddSingleton<ProductService, ProductService>();
            // services.AddSingleton(typeof(ProductService));
            services.AddSingleton(typeof(ProductService), typeof(ProductService));
            services.AddSingleton<PlanetService>();

            // Register Identity
            services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            // Custome config for Identity:
            services.Configure<IdentityOptions>(options =>
            {
                // Password
                options.Password.RequireDigit = false; // Not require number
                options.Password.RequireLowercase = false; // Not require lowercase
                options.Password.RequireNonAlphanumeric = false; // Not require special characters
                options.Password.RequireUppercase = false; // Not require uppercase
                options.Password.RequiredLength = 3; // Minimum 3 chars
                options.Password.RequiredUniqueChars = 1; // Unique chars

                // Setup lockout (lockoutOnFailure = true) & Lock user if user login fail more than 5 times in 5 minutes
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Lock 5 minutes
                options.Lockout.MaxFailedAccessAttempts = 5; // Login fail 5 times
                options.Lockout.AllowedForNewUsers = true;

                // Create User
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // UserName only include these chars
                options.User.RequireUniqueEmail = true;  // Email is unique

                // Login
                options.SignIn.RequireConfirmedEmail = true; // Confirm email is exists
                options.SignIn.RequireConfirmedPhoneNumber = false; // Confirm phone number
                options.SignIn.RequireConfirmedAccount = true; // Default false. When register, auto login, not confirm
            });

            // Config Authorize
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/access-denied";
            });

            // Create Credentials in https://console.cloud.google.com/ & Config ClientId, ClientSecret
            services.AddAuthentication()
                    .AddGoogle(options =>
                    {
                        var googleConfig = Configuration.GetSection("Authentication:Google");
                        options.ClientId = googleConfig["ClientId"];
                        options.ClientSecret = googleConfig["ClientSecret"];
                        options.CallbackPath = "/login-with-google"; // default: https://localhost:5001/signin-google
                        // When User accept Google login, it will redirect to /login-with-google?token and save token to session
                    })
                    .AddFacebook(options => // Config Facebook login
                    {
                        var facebookConfig = Configuration.GetSection("Authentication:Facebook");
                        options.AppId = facebookConfig["AppId"];
                        options.AppSecret = facebookConfig["AppSecret"];
                        options.CallbackPath = "/login-with-facebook";
                    })
                    // .AddTwitter()
                    // .AddMicrosoftAccount()
                    ;

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

            app.AddStatusCodePage(); // Custom Response Code 400 - 599

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // /sayhi
                endpoints.MapGet("/sayhi", async (context) =>
                {
                    await context.Response.WriteAsync($"Hello ASP.NET MVC {DateTime.Now}");
                });

                // 5.1 endpoints.MapControllerRoute
                // URL = /any-word/1
                endpoints.MapControllerRoute(
                    name: "first",
                    pattern: "view-product/{id:range(2,4)}", // route constraint reference to convert new RangeRouteConstraint(2, 4) to range(2,4)
                    defaults: new
                    {
                        controller = "First",
                        action = "ViewProduct"
                    }
                    // constraints: new
                    // {
                    //     // IRouteConstraint to limit route
                    //     url = "view-product",
                    //     id = new RangeRouteConstraint(2, 4)
                    // }
                );

                // 5.2.1 endpoints.MapAreaControllerRoute
                endpoints.MapAreaControllerRoute(
                    name: "product",
                    pattern: "/{controller}/{action=Index}/{id?}",
                    areaName: "ProductManage"
                );

                // URL = /{controller}/{action}/{id?}
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "/{controller=Home}/{action=Index}/{id?}"
                );

                // endpoints.MapControllers
                // endpoints.MapDefaultControllerRoute

                // [AcceptVerbs] -> use for action [AcceptVerbs("POST", "GET")]
                // [Route]
                // [HttpGet]
                // [HttpPost]
                // [HttpPut]
                // [HttpDelete]
                // [HttpHead]
                // [HttpPatch]

                endpoints.MapRazorPages();
            });
        }
    }
}

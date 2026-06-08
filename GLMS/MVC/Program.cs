using Global_Logistics_Management_System___ST10439898.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace Global_Logistics_Management_System___ST10439898
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "CookieAuth";
            })
            .AddCookie("CookieAuth", options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            });

            builder.Services.AddAuthorization();

            // Add distributed memory cache for session
            builder.Services.AddDistributedMemoryCache();

            //for the API
            builder.Services.AddHttpClient<ClientApiService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7048");
            });

            builder.Services.AddHttpClient<ContractApiService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7048");
            });

            builder.Services.AddHttpClient<ServiceRequestApiService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7048");
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSession();

            // make it so that the user has to login before anything else
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
using HUBShop.Models;
using HUBShop.Models.Users;
using HUBShop.Servises;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace HUBShop
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews(options => {
                options.CacheProfiles.Add("Cashing", new CacheProfile() { Location = ResponseCacheLocation.Any, Duration = 300 });
                options.CacheProfiles.Add("NoCashing", new CacheProfile() { Location = ResponseCacheLocation.None, NoStore = true });
            });
            builder.Services.Configure<BrotliCompressionProviderOptions>(options => {
                options.Level = CompressionLevel.Optimal;
            });
            builder.Services.Configure<GzipCompressionProviderOptions>(options => {
                options.Level = CompressionLevel.Optimal;
            });
            builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            string connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<HubContext>(options => options.UseNpgsql(connection))
                .AddIdentity<User, IdentityRole<int>>(options =>
                {
                    options.Password.RequiredLength = 5;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireDigit = false;
                }).AddEntityFrameworkStores<HubContext>();
            builder.Services.AddTransient<UserService>();
            builder.Services.AddMemoryCache();
            var app = builder.Build();
            app.UseResponseCompression();
            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                await AdminInitial.SeedAdminUser(roleManager, userManager);
            }
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
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Goals}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Utility;
using Shared;
using DataAccess.Repository.IRepository;
using DataAccess.Repository;
using System.Configuration;
using Twilio;
using MoveTime.Hubs;

namespace MoveTime
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();

            #region DataBase
            builder.Services.AddDbContext<ApplicationDbContext>(options
                => options.UseSqlServer(builder.Configuration
                .GetConnectionString("DefaultConnection")));
            #endregion
            #region Identity
            builder.Services.AddDefaultIdentity<ApplicationUser>(options
                => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            #endregion
            #region Email Sender
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            #endregion
            #region Register With
            builder.Services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId =
                "537384473279-6joarsu0rdkhknsieihl462l1hul2l8i.apps.googleusercontent.com";
                googleOptions.ClientSecret =
                "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";
            });
            #endregion
            #region Unit of Work
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<UnitOfWork>();
            #endregion
            #region Twilio
            builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));
            builder.Services.AddScoped<WhatsAppService>();
            #endregion
            #region Base Service
            builder.Services.AddScoped<BaseService>();
            builder.Services.AddScoped<CalculatePrice>();
            builder.Services.AddScoped<SubscriptionCheck>();
            builder.Services.AddScoped<Statistics>();
            #endregion

            var app = builder.Build();

            #region Inteal Create Roles And 1st Admin

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await IntailRolesAdmin.SeedRolesAsync(services);
                await IntailRolesAdmin.SeedAdminUserAsync(services);
            }
            #endregion

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

            #region Map SignalR hub route

            app.MapHub<CheckInOutHub>("/checkInOutHub");
            app.MapHub<CardHub>("/cardHub");
            app.MapHub<LoggedInHub>("/loggedInHub");
            app.MapHub<DocHub>("/docHub");
            app.MapHub<StatisticsHub>("/statisticsHub");
            app.MapHub<CheckChildData>("/checkChildData");

            #endregion
            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

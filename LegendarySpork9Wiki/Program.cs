using LegendarySpork9Wiki.Components;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Bind AppSettings
            SharedSettingsModel sharedSettings = new SharedSettingsModel();
            builder.Configuration.GetSection("AppSettings").Bind(sharedSettings);

            // Register services
            builder.Services.AddSingleton(sharedSettings);
            builder.Services.AddSingleton<APIService>();
            builder.Services.AddSingleton<TemplateService>();
            builder.Services.AddSingleton<LoggerService>();
            builder.Services.AddScoped<UserModel>();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}

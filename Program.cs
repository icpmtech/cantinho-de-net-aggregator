using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
using MarketAnalyticHub.Services.Jobs;
using Microsoft.Extensions.DependencyInjection;
using MarketAnalyticHub.Services.News;
using MarketAnalyticHub.Controllers.AIPilot;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MarketAnalyticHub.Models;
using ApplicationDbContext = MarketAnalyticHub.Models.SetupDb.ApplicationDbContext;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Controllers;
using AspnetCoreMvcFull.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to localization.
builder.Services.AddControllersWithViews()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
  var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("en"),
                new CultureInfo("fr"),
                new CultureInfo("de"),
                new CultureInfo("pt")
            };

  options.DefaultRequestCulture = new RequestCulture("en");
  options.SupportedCultures = supportedCultures;
  options.SupportedUICultures = supportedCultures;
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add services to the container.
builder.Services.AddHttpClient<NewsService>(client =>
{
  client.BaseAddress = new Uri("https://localhost:7230/"); // Replace with your actual base address
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppIdentityDbContext>();



builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<SentimentAnalysisService>();
builder.Services.AddTransient<NewsScraper>();
builder.Services.AddScoped<AppNewsService>();
builder.Services.AddScoped<PortfolioService>();
builder.Services.AddScoped<PortfolioItemService>();
builder.Services.AddScoped<AspnetCoreMvcFull.Services.ISymbolService, AspnetCoreMvcFull.Services.SymbolService>();
builder.Services.AddScoped<IQualitativeEventService, QualitativeEventService>();
builder.Services.AddScoped<ISymbolRepository, SymbolRepository>();
builder.Services.AddScoped<SymbolService>();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseDefaultTypeSerializer()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
      CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
      SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
      QueuePollInterval = TimeSpan.Zero,
      UseRecommendedIsolationLevel = true,
      UsePageLocksOnDequeue = true,
      DisableGlobalLocks = true
    }));

builder.Services.AddAuthorization();
// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

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

// Hangfire Dashboard
app.UseHangfireDashboard();
app.UseHangfireServer();

// Schedule recurring job
using (var scope = app.Services.CreateScope())
{
  var serviceProvider = scope.ServiceProvider;
  var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
  recurringJobManager.AddOrUpdate(
      "scrape-news",
      () => serviceProvider.GetService<NewsScraper>().ScrapeNewsAsync(),
      Cron.Hourly); // or any cron expression
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.UseEndpoints(endpoints =>
{
  endpoints.MapHub<ChatHub>("/chathub");
});

app.Run();

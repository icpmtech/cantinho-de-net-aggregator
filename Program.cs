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
using MarketAnalyticHub.Controllers;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using MarketAnalyticHub;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using MarketAnalyticHub.Controllers;
using AspnetCoreMvcFull.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                  options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches()
    .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();

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
var baseEndpoint = builder.Configuration.GetSection("NewsScrapingEndpoints")["BaseEndpoint"];

builder.Services.AddHttpClient<NewsService>(client =>
{
  client.BaseAddress = new Uri(baseEndpoint);
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<SentimentAnalysisService>();
builder.Services.AddTransient<NewsScraper>();
builder.Services.AddScoped<AppNewsService>();
builder.Services.AddScoped<PortfolioService>();
builder.Services.AddScoped<PortfolioItemService>();
builder.Services.AddScoped<MarketAnalyticHub.Services.ISymbolService, MarketAnalyticHub.Services.SymbolService>();
builder.Services.AddScoped<IQualitativeEventService, QualitativeEventService>();
builder.Services.AddScoped<ISymbolRepository, SymbolRepository>();
builder.Services.AddScoped<SymbolService>();
builder.Services.AddHttpClient<AlphaVantageService>();
builder.Services.AddHttpClient<FinnhubService>();
builder.Services.AddHttpClient<BarchartService>();
builder.Services.AddScoped<IYahooFinanceService, YahooFinanceService>();
builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<SocialSentimentService>();
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
builder.Services.AddHangfireServer();

// Register the Swagger generator, defining one or more Swagger documents
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Markets Analytics Hub API", Version = "v1" });
  c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
  
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}
else
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
};

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "Markets Analytics Hub API");
  c.RoutePrefix = "api-docs"; // Set Swagger UI at the app's root
});

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
      Cron.Daily); // or any cron expression
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.UseEndpoints(endpoints =>
{
  endpoints.MapHub<ChatHub>("/chathub");
  endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
  );
});

app.MapCompanyEndpoints();

app.Run();

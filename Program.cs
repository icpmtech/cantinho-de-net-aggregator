using AspnetCoreMvcFull.Models.SetupDb;
using AspnetCoreMvcFull.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
using AspnetCoreMvcFull.Services.Jobs;
using Microsoft.Extensions.DependencyInjection;
using AspnetCoreMvcFull.Services.News;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add services to the container.
builder.Services.AddHttpClient<NewsService>(client =>
{
  client.BaseAddress = new Uri("https://localhost:7230/"); // Replace with your actual base address
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<SentimentAnalysisService>();
builder.Services.AddTransient<NewsScraper>();
builder.Services.AddScoped<AppNewsService>();
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
// Hangfire Dashboard
app.UseHangfireDashboard();
app.UseHangfireServer();

// Schedule recurring job
var serviceProvider = app.Services;
var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate(
        "scrape-news",
        () => serviceProvider.GetService<NewsScraper>().ScrapeNewsAsync(),
        Cron.Hourly); // or any cron expression


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboards}/{action=Index}/{id?}");

app.Run();

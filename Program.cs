using AspnetCoreMvcFull.Models.SetupDb;
using AspnetCoreMvcFull.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
using AspnetCoreMvcFull.Services.Jobs;
using Microsoft.Extensions.DependencyInjection;
using AspnetCoreMvcFull.Services.News;
using System.Configuration;
using AspnetCoreMvcFull.Controllers.AIPilot;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AspnetCoreMvcFull.Models;
using ApplicationDbContext = AspnetCoreMvcFull.Models.SetupDb.ApplicationDbContext;

var builder = WebApplication.CreateBuilder(args);

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
           .AddEntityFrameworkStores<AppIdentityDbContext>()
           .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
  options.LoginPath = "/Account/Login";
  options.LogoutPath = "/Account/Logout";
})
.AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Issuer"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
  };
});

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<SentimentAnalysisService>();
builder.Services.AddTransient<NewsScraper>();
builder.Services.AddScoped<AppNewsService>();
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
var serviceProvider = app.Services;
var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate(
        "scrape-news",
        () => serviceProvider.GetService<NewsScraper>().ScrapeNewsAsync(),
        Cron.Hourly); // or any cron expression



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.UseEndpoints(endpoints =>
{
  endpoints.MapHub<ChatHub>("/chathub");
});
app.Run();

using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.WatermarkApp.BackgroundServices;
using RabbitMQ.WatermarkApp.Models;
using RabbitMQ.WatermarkApp.Services;

var builder = WebApplication.CreateBuilder(args);

var aa = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
IConfiguration configuration = aa.Build();

string rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ") ?? "";

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase(databaseName: "ProductDb");
});
builder.Services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(rabbitMqConnectionString), DispatchConsumersAsync = true});
builder.Services.AddSingleton<RabbitMQClientService>();
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddHostedService<ImageWatermarkProcessBackgroundService>();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

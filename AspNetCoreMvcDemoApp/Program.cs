using AspNetCoreMvcDemoApp.Core.Web.Middleware.Extensions;
using Microsoft.AspNetCore.Http.Features;
using SadLib.Infrastructure.DependencyInjection;
using SadLib.Infrastructure.Storage.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    // Set the maximum allowed file size for uploads to 10 MB.
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

// Some special services need HttpContext.
builder.Services.AddHttpContextAccessor();

// Set Controllers options
builder.Services.AddControllersWithViews()
    .AddViewOptions(options =>
    {
        // Disable client validation like JQuery.
        options.HtmlHelperOptions.ClientValidationEnabled = false;
    }).AddJsonOptions(options => { 
        // Configure Global JSON serialization options.
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.Converters.Add(new SmartDateTimeConverter());
    });

// SadLib Framework Setup
builder.Services.AddSadLib(options =>
{
    options
    .UseMapper(System.Reflection.Assembly.GetExecutingAssembly(), $"{nameof(AspNetCoreMvcDemoApp)}.Models")
    .UseStorage(new PhysicalStorageProviderFactory(builder.Environment.WebRootPath))
    .UseDbService(Microsoft.Data.SqlClient.SqlClientFactory.Instance)
    .UseUpload();
});


// Register custom services.
builder.Services.AddApplicationInfrastructure();

var app = builder.Build();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseCustomMiddlewares();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();

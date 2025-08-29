using ProjectForInizio.Services;

// Create a builder for the web app and load configuration, logging, DI container, etc.
var builder = WebApplication.CreateBuilder(args);

// Register MVC controller + views support
builder.Services.AddControllersWithViews();

// Register our search service as a *typed HttpClient*.
// ASP.NET Core DI will inject an HttpClient that is pooled/reused,
// and construct GoogleSearchService wherever IGoogleSearchService is requested.
builder.Services.AddHttpClient<IGoogleSearchService, GoogleSearchService>();

var app = builder.Build();

// In Production, show a friendly error page and enable HSTS (security)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Security + static files + routing pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Conventional route: default goes to Search/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Search}/{action=Index}/{id?}"
);

// Start Kestrel HTTP server and block this thread
app.Run();

// Expose an empty partial Program class so integration tests can find the entry point
public partial class Program { }

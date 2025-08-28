
using ProjectForInizio.Services;


    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews();
// builder.Services.AddScoped<IGoogleSearchService, FakeGoogleSearchService>();
builder.Services.AddHttpClient<IGoogleSearchService, GoogleSearchService>();



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Search}/{action=Index}/{id?}"
);

app.Run();

public partial class Program { } // for integration tests

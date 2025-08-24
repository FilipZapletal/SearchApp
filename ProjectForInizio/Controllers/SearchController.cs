using Microsoft.AspNetCore.Mvc;
using ProjectForInizio.Services;

namespace ProjectForInizio.Controllers;

public class SearchController : Controller
{
    private readonly IGoogleSearchService _service;
    private readonly IWebHostEnvironment _env;

    // DI injects both the search service and the host environment
    public SearchController(IGoogleSearchService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run(string query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query)) return View("Index");

        try
        {
            var result = await _service.SearchAsync(query, ct);
            ViewBag.Query = result.Query;
            ViewBag.Items = result.Items;

            // 🔹 Save JSON file
            var webroot = _env.WebRootPath ?? "wwwroot";   // fallback if null
            var dir = Path.Combine(webroot, "results");
            Directory.CreateDirectory(dir);

            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Slug(result.Query)}.json";
            var path = Path.Combine(dir, fileName);

            await System.IO.File.WriteAllTextAsync(
                path,
                System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
                ct);

            // Give view a download link
            ViewBag.DownloadUrl = Url.Content($"/results/{fileName}");
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.ToString();
        }

        return View("Index");
    }

    private static string Slug(string s) =>
        new string(s.ToLowerInvariant().Where(ch => char.IsLetterOrDigit(ch) || ch == '-').ToArray());

    [HttpGet("/diag")]
    public IActionResult Diag([FromServices] IConfiguration cfg)
    {
        var k = cfg["Google:ApiKey"];
        var cx = cfg["Google:Cx"];
        var hasKey = !string.IsNullOrWhiteSpace(k);
        var hasCx = !string.IsNullOrWhiteSpace(cx);
        return Content($"HasKey={hasKey} LenKey={(k?.Length ?? 0)} | HasCx={hasCx} LenCx={(cx?.Length ?? 0)}");
    }

}

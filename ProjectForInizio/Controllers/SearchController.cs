using Microsoft.AspNetCore.Mvc;
using ProjectForInizio.Services;

namespace ProjectForInizio.Controllers;

public class SearchController : Controller
{
    private readonly IGoogleSearchService _service;          // abstraction for search
    private readonly IWebHostEnvironment _env;               // gives us wwwroot path etc.

    // DI: ASP.NET Core creates this controller and injects the registered services.
    public SearchController(IGoogleSearchService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    // GET /Search  — show the form
    [HttpGet]
    public IActionResult Index() => View();

    // POST /Search/Run — handle the submitted query
    // Anti-forgery: blocks cross-site form posts unless token is present and valid.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run(string query, CancellationToken ct)
    {
        // Basic validation: reject empty input and redisplay form.
        if (string.IsNullOrWhiteSpace(query))
        {
            // Optional: tell the view to show a message
            // ModelState.AddModelError("query", "Query is required.");
            return View("Index");
        }

        try
        {
            // Call the external Google Custom Search API via our service
            var result = await _service.SearchAsync(query, ct);

            // Pass data to the view (for a stricter approach, use a ViewModel instead of ViewBag)
            ViewBag.Query = result.Query;
            ViewBag.Items = result.Items;

            // Persist a pretty-printed JSON copy under wwwroot/results so it’s downloadable
            var webroot = _env.WebRootPath ?? "wwwroot";
            var dir = Path.Combine(webroot, "results");
            Directory.CreateDirectory(dir);

            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Slug(result.Query)}.json";
            var path = Path.Combine(dir, fileName);

            await System.IO.File.WriteAllTextAsync(
                path,
                System.Text.Json.JsonSerializer.Serialize(
                    result,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
                ct);

            // Provide a link the user can click to download the JSON
            ViewBag.DownloadUrl = Url.Content($"/results/{fileName}");
        }
        catch (Exception ex)
        {
            // Bubble the error to the page for debugging (safe enough for this demo app)
            ViewBag.Error = ex.ToString();
        }

        // Always render the Index view (either with results or with an error)
        return View("Index");
    }

    // Create a safe filename fragment from the query (lowercase letters/digits/hyphens only)
    private static string Slug(string s) =>
        new string(s.ToLowerInvariant().Where(ch => char.IsLetterOrDigit(ch) || ch == '-').ToArray());

    // Quick diagnostics endpoint to confirm secrets are wired
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

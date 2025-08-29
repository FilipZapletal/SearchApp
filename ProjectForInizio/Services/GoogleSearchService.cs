using System.Text.Json;
using ProjectForInizio.Dtos;

namespace ProjectForInizio.Services;

/// <summary>
/// Service that calls the Google Custom Search API and converts the JSON
/// response into our own DTOs.
/// </summary>
public class GoogleSearchService : IGoogleSearchService
{
    //   HttpClient injected by ASP.NET Core DI (AddHttpClient)
    //   - keeps TCP connections pooled and reused
    //   - avoids socket exhaustion compared to "new HttpClient()"
    private readonly HttpClient _http;

    //   API key and CX (Search Engine ID) loaded from configuration
    //     - local dev: dotnet user-secrets
    //     - prod: Azure App Settings
    private readonly string _apiKey;
    private readonly string _cx;

    /// <summary>
    /// Constructor. Dependencies are injected by DI.
    /// </summary>
    public GoogleSearchService(HttpClient http, IConfiguration cfg)
    {
        _http = http;

        // Load secrets from configuration. Throw if not present → fail fast.
        _apiKey = cfg["Google:ApiKey"]
            ?? throw new InvalidOperationException("Google:ApiKey missing");

        _cx = cfg["Google:Cx"]
            ?? throw new InvalidOperationException("Google:Cx missing");
    }

    /// <summary>
    /// Perform a search query against the Google Custom Search JSON API.
    /// Returns a SearchResultDto with the normalized data.
    /// </summary>
    public async Task<SearchResultDto> SearchAsync(string query, CancellationToken ct = default)
    {
        // Reject empty/whitespace input early
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query is empty.");

        // Sanity check: ensure secrets exist
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_cx))
            throw new InvalidOperationException("Missing Google:ApiKey or Google:Cx. Check user-secrets / app settings.");

        // Build request URL for Google Custom Search API
        var url =
            $"https://www.googleapis.com/customsearch/v1?key={_apiKey}&cx={_cx}&q={Uri.EscapeDataString(query)}";

        // Send GET request
        using var res = await _http.GetAsync(url, ct);

        // Read body as string (JSON)
        var body = await res.Content.ReadAsStringAsync(ct);

        // If response status is not 200 OK → throw with error details
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"Google API error {(int)res.StatusCode}: {body}");

        // Parse the JSON body into a DOM
        using var doc = JsonDocument.Parse(body);

        // Extract items → map to our DTO
        var items = new List<SearchItemDto>();
        if (doc.RootElement.TryGetProperty("items", out var arr))
        {
            foreach (var it in arr.EnumerateArray())
            {
                var title = it.TryGetProperty("title", out var t) ? t.GetString() : null;
                var link = it.TryGetProperty("link", out var l) ? l.GetString() : null;
                var snippet = it.TryGetProperty("snippet", out var s) ? s.GetString() : null;

                // Only add items that have both title + link
                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(link))
                    items.Add(new SearchItemDto(title!, link!, snippet));
            }
        }

        // Wrap everything in our SearchResultDto and return
        return new SearchResultDto(query, DateTime.UtcNow, items);
    }
}

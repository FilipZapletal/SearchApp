using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

public class IntegrationSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _c;
    public IntegrationSmokeTests(WebApplicationFactory<Program> f) => _c = f.CreateClient();

    [Fact]
    public async Task Home_Index_Returns200()
    {
        var r = await _c.GetAsync("/");
        r.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Search_Index_Returns200()
    {
        var r = await _c.GetAsync("/Search");
        r.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UnknownRoute_Returns404()
    {
        var r = await _c.GetAsync("/__nope__");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }
}

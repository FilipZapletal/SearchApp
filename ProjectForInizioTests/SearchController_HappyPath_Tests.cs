using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectForInizio.Controllers;
using ProjectForInizio.Dtos;
using ProjectForInizio.Services;

public class SearchController_HappyPath_Tests
{
    [Fact]
    public async Task Run_ValidQuery_SetsViewBag_And_ReturnsIndex()
    {
        // temp webroot for file save
        var tmp = Path.Combine(Path.GetTempPath(), "mvc-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(tmp);

        var env = new Mock<IWebHostEnvironment>();
        env.SetupGet(e => e.WebRootPath).Returns(tmp);

        var items = new List<SearchItemDto>
        {
            new("Title 1","https://ex.com/1","Snippet 1"),
            new("Title 2","https://ex.com/2",null)
        };
        var svc = new Mock<IGoogleSearchService>();
        svc.Setup(s => s.SearchAsync("Praha", It.IsAny<CancellationToken>()))
           .ReturnsAsync(new SearchResultDto("Praha", DateTime.UtcNow, items));

        var c = new SearchController(svc.Object, env.Object);

        var result = await c.Run("Praha", CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);

        Assert.Equal("Praha", c.ViewBag.Query);
        var vbItems = (IEnumerable<SearchItemDto>)c.ViewBag.Items;
        Assert.Equal(2, vbItems.Count());

        // optional: quick file existence check (no need to read JSON)
        var dir = Path.Combine(tmp, "results");
        Assert.True(Directory.Exists(dir));
        Assert.Single(Directory.GetFiles(dir, "*.json"));

        Directory.Delete(tmp, true);
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectForInizio.Controllers;
using ProjectForInizio.Services;

public class SearchController_BadInput_Tests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Run_EmptyOrWhitespace_ReturnsIndex(string input)
    {
        var env = new Mock<IWebHostEnvironment>();
        var svc = new Mock<IGoogleSearchService>(MockBehavior.Strict); // should not be called
        var c = new SearchController(svc.Object, env.Object);

        var result = await c.Run(input, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
    }
}

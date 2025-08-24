using Microsoft.AspNetCore.Mvc;
using ProjectForInizio.Services;

namespace ProjectForInizio.Controllers
{
    public class SearchController : Controller
    {
        private readonly IGoogleSearchService _service;

        public SearchController(IGoogleSearchService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Search/Run
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(string query, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ModelState.AddModelError(string.Empty, "Query is required.");
                return View("Index");
            }
            var result = await _service.SearchAsync(query, ct);
            ViewBag.Query = result.Query;
            ViewBag.Result = result.Items;
            return View("Index");
        }

    }
}

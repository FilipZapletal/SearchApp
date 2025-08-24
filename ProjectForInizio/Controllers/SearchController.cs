using Microsoft.AspNetCore.Mvc;
using ProjectForInizio.Dtos;
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
                return View("Index");
            }
            try
            {
                var result = await _service.SearchAsync(query, ct);
                ViewBag.Query = result.Query;
                ViewBag.Items = result.Items;
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.ToString();
            }
            return View("Index");

        }

    }
}

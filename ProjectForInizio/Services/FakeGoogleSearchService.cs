using ProjectForInizio.Dtos;

namespace ProjectForInizio.Services
{
    public class FakeGoogleSearchService : IGoogleSearchService
    {
        public Task<SearchResultDto> SearchAsync(string query, CancellationToken ct = default)
        {
            // vytvořit nějaké falešné výsledky
            var items = new List<SearchItemDto>
            {
                new SearchItemDto("Example Title 1", "https://example.com/1", "This is a snippet for example 1."),
                new SearchItemDto("Example Title 2", "https://example.com/2", "This is a snippet for example 2."),
                new SearchItemDto("Example Title 3", "https://example.com/3", "This is a snippet for example 3."),
            };

            //finalní dto objekt
            var dto = new SearchResultDto(
                Query: query,
                UtcFetchedAt: DateTime.UtcNow,
                Items: items
            );

            // vrátit jako Task protože metoda je asynchronní
            return Task.FromResult(dto);
        }
    }
}

using ProjectForInizio.Dtos;

namespace ProjectForInizio.Services
{
    public interface IGoogleSearchService
    {
        Task<SearchResultDto> SearchAsync(string query, CancellationToken ct = default);
    }
}

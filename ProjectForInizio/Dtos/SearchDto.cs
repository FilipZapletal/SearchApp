namespace ProjectForInizio.Dtos;

public record SearchItemDto(string Title, string Url, string? Snippet);

public record SearchResultDto(
    string Query,
    DateTime UtcFetchedAt,
    List<SearchItemDto> Items
);

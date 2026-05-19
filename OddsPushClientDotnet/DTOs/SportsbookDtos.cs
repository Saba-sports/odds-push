namespace OddsPushClient.DTOs;

public record EventDto
{
    public long EventId { get; init; }
    public int SportType { get; init; }
    public string LeagueName { get; init; } = string.Empty;
    public string HomeTeamName { get; init; } = string.Empty;
    public string AwayTeamName { get; init; } = string.Empty;
    public string? KickoffTime { get; init; }
    public string? EventStatus { get; init; }
    public int LiveHomeScore { get; init; }
    public int LiveAwayScore { get; init; }
    public string? CurrentPeriod { get; set; }
    public bool IsLive { get; init; }
    public List<MarketDto> Markets { get; init; } = new();
}

public record MarketDto
{
    public long MarketId { get; init; }
    public int BetType { get; init; }
    public string BetTypeName { get; init; } = string.Empty;
    public string MarketStatus { get; init; } = string.Empty;
    public List<SelectionDto> Selections { get; init; } = new();
}

public record SelectionDto
{
    public string SelectionKey { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? Point { get; init; }
}

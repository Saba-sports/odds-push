using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OddsPushClient.Data;

public class SportEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long EventId { get; set; }
    public int SportType { get; set; }
    public int LeagueId { get; set; }
    public string LeagueName { get; set; } = string.Empty;
    public string HomeTeamName { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;
    public string? KickoffTime { get; set; }
    public string? EventStatus { get; set; }
    public bool IsLive { get; set; }
    public long VersionKey { get; set; }
    public DateTime? ClosedAt { get; set; }

    // State information
    public int LiveHomeScore { get; set; }
    public int LiveAwayScore { get; set; }
    public string? CurrentPeriod { get; set; }

    public List<Market> Markets { get; set; } = new();
}

public class Market
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long MarketId { get; set; }
    public long EventId { get; set; }
    public int BetType { get; set; }
    public string MarketStatus { get; set; } = string.Empty;
    public long VersionKey { get; set; }
    public bool IsLive { get; set; }
    public DateTime? ClosedAt { get; set; }

    public List<Selection> Selections { get; set; } = new();

    [ForeignKey("EventId")]
    public SportEvent? Event { get; set; }
}

public class Selection
{
    public int Id { get; set; }
    public long MarketId { get; set; }
    public string SelectionKey { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? Point { get; set; }

    [ForeignKey("MarketId")]
    public Market? Market { get; set; }
}

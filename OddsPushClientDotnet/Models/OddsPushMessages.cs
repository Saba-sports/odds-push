using System;
using System.Collections.Generic;

namespace OddsPushClient.Models;

#region Envelopes (Non-Generic)
public record OddsPushEventEnvelope
{
    public int messageType { get; init; }
    public OddsPushEvent? data { get; init; }
}

public record OddsPushEventStateEnvelope
{
    public int messageType { get; init; }
    public OddsPushEventState? data { get; init; }
}

public record OddsPushMarketEnvelope
{
    public int messageType { get; init; }
    public OddsPushMarket? data { get; init; }
}

public record OddsPushHeartbeatEnvelope
{
    public int messageType { get; init; }
    public HeartbeatMessage? data { get; init; }
}
#endregion

#region 4.1 賽事模型 (Event)
public record OddsPushEvent
{
    public long eventId { get; init; }
    public int leagueId { get; init; }
    public int sportType { get; init; }
    public int homeTeamId { get; init; }
    public int awayTeamId { get; init; }
    public string leagueName { get; init; } = string.Empty;
    public string homeTeamName { get; init; } = string.Empty;
    public string awayTeamName { get; init; } = string.Empty;
    public string? kickoffTime { get; init; }
    public string? eventStatus { get; init; }
    public int livePeriod { get; init; }
    public bool isLive { get; init; }
    public long versionKey { get; init; }
    public List<StreamingLink>? streamingLinks { get; init; }
}

public record StreamingLink
{
    public string provider { get; init; } = string.Empty;
    public string url { get; init; } = string.Empty;
    public string language { get; init; } = string.Empty;
}
#endregion

#region 4.2 狀態模型 (EventState)
public record OddsPushEventState
{
    public long eventId { get; init; }
    public int sportType { get; init; }
    public string? marketType { get; init; }
    public int liveHomeScore { get; init; }
    public int liveAwayScore { get; init; }
    public long versionKey { get; init; }
    public BasketBallLiveScore? basketBallLiveScore { get; init; }
    public FootballLiveScore? footballLiveScore { get; init; }
}

public record BasketBallLiveScore
{
    public string? a1q { get; init; }
    public string? h1q { get; init; }
    public string? llp { get; init; }
}

public record FootballLiveScore
{
    public int h1q { get; init; }
    public int a1q { get; init; }
}
#endregion

#region 4.3 盤口模型 (Market)
public record OddsPushMarket
{
    public long eventID { get; init; }
    public int sportType { get; init; }
    public string marketType { get; init; } = string.Empty;
    public List<MarketDetail> markets { get; init; } = new();
}

public record MarketDetail
{
    public long marketID { get; init; }
    public int betType { get; init; }
    public string marketStatus { get; init; } = string.Empty;
    public long versionKey { get; init; }
    public List<SelectionDetail> selections { get; init; } = new();
}

public record SelectionDetail
{
    public string key { get; init; } = string.Empty;
    public decimal price { get; init; }
    public decimal? point { get; init; }
    public SelectionPrices? allPrice { get; init; }
}

public record SelectionPrices
{
    public decimal decimalPrice { get; init; }
}
#endregion

public record HeartbeatMessage
{
    public string status { get; init; } = "Alive";
}

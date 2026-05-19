using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OddsPushClient.Data;
using OddsPushClient.Models;
using OddsPushClient.Services;

namespace OddsPushClient.Consumers;

public class RawMessageConsumer
{
    private readonly ILogger<RawMessageConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHeartbeatMonitor _heartbeatMonitor;
    private readonly JsonSerializerOptions _jsonOptions;

    public RawMessageConsumer(
        ILogger<RawMessageConsumer> logger,
        IServiceScopeFactory scopeFactory,
        IHeartbeatMonitor heartbeatMonitor)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _heartbeatMonitor = heartbeatMonitor;
        _logger.LogInformation("RawMessageConsumer instantiated.");

        // Fix: Enable string-to-number conversion for incoming RabbitMQ messages
        _jsonOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task HandleMessageAsync(string message)
    {
        try
        {
            using var doc = JsonDocument.Parse(message);

            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("message", out var nestedMessage))
            {
                root = nestedMessage;
            }

            if (!root.TryGetProperty("messageType", out var typeProp)) return;

            var messageType = typeProp.ValueKind == JsonValueKind.Number
                ? typeProp.GetInt32()
                : 0;

            if (messageType == 3)
            {
                _heartbeatMonitor.RecordHeartbeat();
                await CleanupClosedDataAsync();
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SportsbookDbContext>();

            switch (messageType)
            {
                case 0: // Event
                    var eventData = DeserializeData<OddsPushEvent>(root);
                    if (eventData is { sportType: 1 } or { sportType: 2 })
                    {
                        await ProcessEventAsync(dbContext, eventData);
                    }
                    break;

                case 1: // EventState
                    var stateData = DeserializeData<OddsPushEventState>(root);
                    if (stateData is { sportType: 1 } or { sportType: 2 })
                    {
                        await ProcessStateAsync(dbContext, stateData);
                    }
                    break;

                case 2: // Market
                    var marketData = DeserializeData<OddsPushMarket>(root);
                    if (marketData is { sportType: 1 } or { sportType: 2 })
                    {
                        await ProcessMarketAsync(dbContext, marketData);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", message);
        }
    }

    private T? DeserializeData<T>(JsonElement root)
    {
        if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind != JsonValueKind.Null)
        {
            return JsonSerializer.Deserialize<T>(dataProp.GetRawText(), _jsonOptions);
        }
        return default;
    }

    private async Task ProcessEventAsync(SportsbookDbContext db, OddsPushEvent data)
    {
        var existing = await db.Events.FindAsync(data.eventId);
        if (existing == null)
        {
            var newEvent = new SportEvent
            {
                EventId = data.eventId,
                SportType = data.sportType,
                LeagueId = data.leagueId,
                LeagueName = data.leagueName,
                HomeTeamName = data.homeTeamName,
                AwayTeamName = data.awayTeamName,
                KickoffTime = data.kickoffTime,
                EventStatus = data.eventStatus,
                IsLive = data.isLive,
                CurrentPeriod = data.livePeriod.ToString(),
                VersionKey = data.versionKey,
                ClosedAt = data.eventStatus?.Equals("closed", StringComparison.OrdinalIgnoreCase) == true ? DateTime.UtcNow : null
            };
            db.Events.Add(newEvent);
        }
        else if (data.versionKey >= existing.VersionKey)
        {
            existing.LeagueName = data.leagueName;
            existing.HomeTeamName = data.homeTeamName;
            existing.AwayTeamName = data.awayTeamName;
            existing.KickoffTime = data.kickoffTime;

            // Handle ClosedAt logic
            if (data.eventStatus?.Equals("closed", StringComparison.OrdinalIgnoreCase) == true)
            {
                existing.ClosedAt ??= DateTime.UtcNow;
            }
            else
            {
                existing.ClosedAt = null;
            }

            existing.EventStatus = data.eventStatus;
            existing.IsLive = data.isLive;
            existing.CurrentPeriod = data.livePeriod.ToString();
            existing.VersionKey = data.versionKey;
        }
        await db.SaveChangesAsync();
    }

    private async Task ProcessStateAsync(SportsbookDbContext db, OddsPushEventState data)
    {
        var existing = await db.Events.FindAsync(data.eventId);
        if (existing != null && data.versionKey >= existing.VersionKey)
        {
            existing.LiveHomeScore = data.liveHomeScore;
            existing.LiveAwayScore = data.liveAwayScore;
            existing.VersionKey = data.versionKey;
            await db.SaveChangesAsync();
        }
    }

    private async Task ProcessMarketAsync(SportsbookDbContext db, OddsPushMarket data)
    {
        // 檢查 Event 是否存在
        var eventExists = await db.Events.AnyAsync(e => e.EventId == data.eventID);
        if (!eventExists)
        {
            return;
        }

        foreach (var marketDetail in data.markets)
        {
            var existingMarket = await db.Markets
                .Include(m => m.Selections)
                .FirstOrDefaultAsync(m => m.MarketId == marketDetail.marketID);

            if (existingMarket == null)
            {
                var newMarket = new Market
                {
                    MarketId = marketDetail.marketID,
                    EventId = data.eventID,
                    BetType = marketDetail.betType,
                    MarketStatus = marketDetail.marketStatus,
                    VersionKey = marketDetail.versionKey,
                    ClosedAt = marketDetail.marketStatus.Equals("closed", StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow : null
                };

                foreach (var sel in marketDetail.selections)
                {
                    newMarket.Selections.Add(new Selection
                    {
                        MarketId = marketDetail.marketID,
                        SelectionKey = sel.key,
                        Price = sel.price,
                        Point = sel.point
                    });
                }
                db.Markets.Add(newMarket);
            }
            else if (marketDetail.versionKey >= existingMarket.VersionKey)
            {
                // Handle ClosedAt logic
                if (marketDetail.marketStatus.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    existingMarket.ClosedAt ??= DateTime.UtcNow;
                }
                else
                {
                    existingMarket.ClosedAt = null;
                }

                existingMarket.MarketStatus = marketDetail.marketStatus;
                existingMarket.VersionKey = marketDetail.versionKey;

                // Update selections
                foreach (var sel in marketDetail.selections)
                {
                    var selection = existingMarket.Selections.FirstOrDefault(o => o.SelectionKey == sel.key);
                    if (selection != null)
                    {
                        selection.Price = sel.price;
                        selection.Point = sel.point;
                    }
                    else
                    {
                        existingMarket.Selections.Add(new Selection
                        {
                            MarketId = marketDetail.marketID,
                            SelectionKey = sel.key,
                            Price = sel.price,
                            Point = sel.point
                        });
                    }
                }
            }
        }
        await db.SaveChangesAsync();
    }

    private async Task CleanupClosedDataAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SportsbookDbContext>();
            var cutoff = DateTime.UtcNow.AddMinutes(-5);

            // 1. Remove closed events
            var closedEvents = await db.Events
                .Where(e => e.ClosedAt != null && e.ClosedAt < cutoff)
                .ToListAsync();

            if (closedEvents.Any())
            {
                db.Events.RemoveRange(closedEvents);
            }

            // 2. Remove closed markets
            var closedMarkets = await db.Markets
                .Where(m => m.ClosedAt != null && m.ClosedAt < cutoff)
                .ToListAsync();

            if (closedMarkets.Any())
            {
                db.Markets.RemoveRange(closedMarkets);
            }

            if (closedEvents.Any() || closedMarkets.Any())
            {
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data cleanup");
        }
    }
}

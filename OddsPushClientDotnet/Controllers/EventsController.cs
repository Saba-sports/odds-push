using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OddsPushClient.Data;
using OddsPushClient.DTOs;
using OddsPushClient.Services;

namespace OddsPushClient.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly SportsbookDbContext _context;
    private readonly IBetTypeService _betTypeService;

    public EventsController(SportsbookDbContext context, IBetTypeService betTypeService)
    {
        _context = context;
        _betTypeService = betTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents([FromQuery] int? sportType, [FromQuery] string? eventStatus)
    {
        var query = _context.Events.AsQueryable();

        if (sportType.HasValue)
        {
            query = query.Where(e => e.SportType == sportType.Value);
        }

        if (!string.IsNullOrEmpty(eventStatus))
        {
            query = query.Where(e => e.EventStatus == eventStatus);
        }

        query = query.Where(e => e.Markets.Any());

        var events = await query
            .Include(e => e.Markets)
            .ThenInclude(m => m.Selections)
            .OrderByDescending(e => e.KickoffTime)
            .ToListAsync();

        var dtos = new List<EventDto>();
        foreach (var e in events)
        {
            dtos.Add(await MapToDtoAsync(e));
        }
        return dtos;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEvent(long id)
    {
        var sportEvent = await _context.Events
            .Include(e => e.Markets)
            .ThenInclude(m => m.Selections)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (sportEvent == null)
        {
            return NotFound();
        }

        return await MapToDtoAsync(sportEvent);
    }

    private async Task<EventDto> MapToDtoAsync(SportEvent e)
    {
        var marketDtos = new List<MarketDto>();
        foreach (var m in e.Markets)
        {
            marketDtos.Add(new MarketDto
            {
                MarketId = m.MarketId,
                BetType = m.BetType,
                BetTypeName = await _betTypeService.GetBetTypeNameAsync(m.BetType),
                MarketStatus = m.MarketStatus,
                Selections = m.Selections.Select(o => new SelectionDto
                {
                    SelectionKey = o.SelectionKey,
                    Price = o.Price,
                    Point = o.Point
                }).ToList()
            });
        }

        return new EventDto
        {
            EventId = e.EventId,
            SportType = e.SportType,
            LeagueName = e.LeagueName,
            HomeTeamName = e.HomeTeamName,
            AwayTeamName = e.AwayTeamName,
            KickoffTime = e.KickoffTime,
            EventStatus = e.EventStatus,
            LiveHomeScore = e.LiveHomeScore,
            LiveAwayScore = e.LiveAwayScore,
            CurrentPeriod = e.CurrentPeriod,
            IsLive = e.IsLive,
            Markets = marketDtos
        };
    }
}

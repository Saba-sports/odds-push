import React from 'react';
import type { SportEvent, Market, Selection } from './types';
import { Shield, Lock, ChevronRight } from 'lucide-react';

interface OddsSelectionProps {
  selection: Selection;
}

const OddsSelection: React.FC<OddsSelectionProps> = ({ selection }) => (
  <button
    className="flex-1 min-h-[48px] bg-slate-50 dark:bg-slate-800 hover:bg-brand hover:text-white dark:hover:bg-brand border border-slate-200 dark:border-slate-700/50 rounded-lg p-2 transition-all duration-200 flex flex-col items-center justify-center group shadow-sm"
  >
    <div className="text-[10px] uppercase font-bold text-slate-400 group-hover:text-white/80 leading-none mb-1 text-center">
      {selection.selectionKey}
      {selection.point !== null && selection.point !== undefined && (
        <span className="ml-1 text-slate-500 dark:text-slate-300 group-hover:text-white italic">
          ({selection.point > 0 ? `+${selection.point}` : selection.point})
        </span>
      )}
    </div>
    <div className="font-extrabold text-base tabular-nums leading-none">
      {selection.price.toFixed(2)}
    </div>
  </button>
);

interface MarketOddsProps {
  market: Market;
}

export const MarketOdds: React.FC<MarketOddsProps> = ({ market }) => {
  const isClosed = market.marketStatus === 'closed';

  return (
    <div className="flex-1 min-w-[140px]">
      <div className="flex justify-between items-center mb-1.5 px-1 shrink-0">
        <span className="text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-widest">
          {market.betTypeName || `Type ${market.betType}`}
        </span>
        {isClosed && (
          <span className="flex items-center gap-1 text-[9px] font-bold text-red-500 uppercase italic">
            <Lock size={10} strokeWidth={3} />
            Suspended
          </span>
        )}
      </div>
      <div className={`flex gap-1.5 relative ${isClosed ? 'opacity-50 grayscale pointer-events-none' : ''}`}>
        {market.selections.map((sel) => (
          <OddsSelection key={sel.selectionKey} selection={sel} />
        ))}
      </div>
    </div>
  );
};

interface EventCardProps {
  event: SportEvent;
  onOpenDetails: (id: number) => void;
}

const EventCard: React.FC<EventCardProps> = ({ event, onOpenDetails }) => {
  const isLive = event.isLive;

  const displayMarkets = React.useMemo(() => {
    return [...event.markets]
      .sort((a, b) => {
        // Priority to 'running'
        const aRunning = a.marketStatus === 'running' ? 0 : 1;
        const bRunning = b.marketStatus === 'running' ? 0 : 1;
        if (aRunning !== bRunning) return aRunning - bRunning;
        // Then by betType asc
        return a.betType - b.betType;
      })
      .slice(0, 1);
  }, [event.markets]);

  return (
    <div className="bg-white dark:bg-dark-surface rounded-xl shadow-lux border border-slate-200 dark:border-slate-800 overflow-hidden mb-5 transition-transform duration-300 hover:border-brand/40 group/card">
      {/* Dynamic Top Bar */}
      <div className={`h-1 w-full ${isLive ? 'bg-brand' : 'bg-slate-300 dark:bg-slate-700'}`}></div>

      {/* League & Meta Header */}
      <div className="px-5 py-3 bg-slate-50/50 dark:bg-slate-900/50 border-b border-slate-100 dark:border-slate-800 flex justify-between items-center">
        <div className="flex items-center gap-2.5">
           <Shield className="text-brand shrink-0" size={14} />
           <span className="text-[11px] font-black text-slate-600 dark:text-slate-300 uppercase tracking-[0.1em] truncate max-w-[200px]">
             {event.leagueName}
           </span>
        </div>
        <div className="flex items-center gap-3">
           {isLive ? (
             <div className="flex items-center gap-1.5 bg-red-600 px-2 py-0.5 rounded shadow-lg shadow-red-600/20">
               <span className="w-1.5 h-1.5 bg-white rounded-full animate-pulse"></span>
               <span className="text-[10px] font-black text-white uppercase tracking-tighter">Live</span>
             </div>
           ) : (
             <span className="text-[10px] font-black text-slate-400 dark:text-slate-500 uppercase tracking-widest">{'Pre-match'}</span>
           )}
           <div className="text-[11px] font-black text-slate-400 dark:text-slate-500 uppercase flex gap-2">
             <span>ID:{event.eventId}</span>
           </div>
        </div>
      </div>

      <div className="p-5 flex flex-col xl:flex-row gap-8">
        {/* Teams Area */}
        <div className="flex-1 min-w-[280px]">
          <div className="grid grid-cols-[1fr_auto] gap-y-4">
            <div className="flex flex-col justify-center">
              <span className="text-lg font-bold text-slate-800 dark:text-slate-100 tracking-tight leading-tight mb-4">{event.homeTeamName}</span>
              <span className="text-lg font-bold text-slate-800 dark:text-slate-100 tracking-tight leading-tight">{event.awayTeamName}</span>
            </div>
            <div className="flex flex-col items-end justify-center bg-slate-100 dark:bg-slate-800/80 px-4 py-2 rounded-lg gap-2 shadow-inner border border-slate-200/50 dark:border-slate-700/50">
              <span className="text-2xl font-black text-brand tabular-nums leading-none tracking-tight">{event.liveHomeScore}</span>
              <div className="w-8 h-px bg-slate-200 dark:bg-slate-700"></div>
              <span className="text-2xl font-black text-brand tabular-nums leading-none tracking-tight">{event.liveAwayScore}</span>
            </div>
          </div>
        </div>

        {/* Separator */}
        <div className="hidden xl:block w-px bg-slate-200 dark:bg-slate-800"></div>

        {/* Odds Area */}
        <div className="flex-[2.5] flex items-start gap-4">
          <div className="flex flex-wrap md:flex-nowrap gap-4 flex-1 items-start pb-1">
            {displayMarkets.length > 0 ? (
              displayMarkets.map((market) => (
                <MarketOdds key={market.marketId} market={market} />
              ))
            ) : (
              <div className="flex-1 flex items-center justify-center bg-slate-50 dark:bg-slate-900/50 rounded-lg p-4 border border-dashed border-slate-200 dark:border-slate-800">
                <span className="text-xs font-bold text-slate-400 uppercase tracking-widest italic opacity-60">Markets Currently Suspended</span>
              </div>
            )}
          </div>

          <button
            onClick={() => onOpenDetails(event.eventId)}
            className="flex items-center gap-1 px-3 py-1.5 bg-slate-100 dark:bg-slate-800 hover:bg-brand hover:text-white dark:hover:bg-brand text-slate-500 dark:text-slate-400 transition-all rounded-lg border border-slate-200 dark:border-slate-700/50 group shrink-0 mt-5"
          >
            <span className="text-[10px] font-black uppercase tracking-widest">More</span>
            <ChevronRight size={12} className="group-hover:translate-x-0.5 transition-transform" />
          </button>
        </div>
      </div>

      {/* Footer Meta */}
      <div className="px-5 py-2 bg-slate-50/30 dark:bg-slate-900/10 border-t border-slate-50 dark:border-slate-800/80 flex justify-between items-center opacity-80">
        <div className="flex items-center gap-1.5 text-[10px] text-slate-400 font-bold uppercase">
          <Shield size={12} strokeWidth={2.5} />
          Verified Feed
        </div>
        <div className="text-[10px] text-slate-400 font-bold uppercase tracking-tight flex items-center gap-1">
          Kickoff: <span className="text-slate-500 dark:text-slate-300">{new Date(event.kickoffTime || Date.now()).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit', hour12: false})} | {new Date(event.kickoffTime || Date.now()).toLocaleDateString()}</span>
        </div>
      </div>
    </div>
  );
};

export default EventCard;
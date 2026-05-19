import React, { useEffect, useState } from 'react';
import { getEvents } from './api';
import type { SportEvent } from './types';
import { Trophy, Activity, AlertCircle, BarChart3, Layout, ChevronRight, CircleDot, Volleyball, Menu, X, Shield } from 'lucide-react';
import EventCard, { MarketOdds } from './EventCard';
import { getEvent } from './api';

const App: React.FC = () => {
  const [events, setEvents] = useState<SportEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedSportType, setSelectedSportType] = useState<number | null>(1);
  const [isSidebarOpen, setIsSidebarOpen] = useState(true);

  // Global Detail Modal State
  const [selectedEventId, setSelectedEventId] = useState<number | null>(null);
  const [detailedEvent, setDetailedEvent] = useState<SportEvent | null>(null);
  const [loadingModal, setLoadingModal] = useState(false);

  const openDetails = async (id: number) => {
    setSelectedEventId(id);
    setLoadingModal(true);
    setDetailedEvent(null);
    try {
      const fullEvent = await getEvent(id);
      setDetailedEvent(fullEvent);
    } catch (err) {
      console.error('Failed to fetch event details:', err);
    } finally {
      setLoadingModal(false);
    }
  };

  const fetchData = async () => {
    try {
      const data = await getEvents(selectedSportType, 'running');
      // Additional filter: only show events that have at least one running market
      const activeData = data.filter(e => e.markets.some(m => m.marketStatus === 'running'));
      const sortedData = [...activeData].sort((a, b) => {
        const aLive = a.isLive;
        const bLive = b.isLive;
        if (aLive && !bLive) return -1;
        if (!aLive && bLive) return 1;
        return new Date(a.kickoffTime || 0).getTime() - new Date(b.kickoffTime || 0).getTime();
      });
      setEvents(sortedData);
      setError(null);
    } catch (err: any) {
      console.error('Error fetching events:', err);
      if (err.response?.status === 503) {
        setError('MAINTENANCE_MODE_ACTIVE');
      } else {
        setError('SYSTEM_CONNECTION_ERROR');
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
    const interval = setInterval(fetchData, 5000);
    return () => clearInterval(interval);
  }, [selectedSportType]);

  const filteredEvents = events;

  const sportCategories = [
    { id: null, name: 'All Sports', icon: Layout, count: events.length },
    { id: 1, name: 'Soccer', icon: CircleDot, count: events.filter(e => e.sportType === 1).length, color: 'text-emerald-500' },
    { id: 2, name: 'Basketball', icon: Volleyball, count: events.filter(e => e.sportType === 2).length, color: 'text-orange-500' },
  ];

  return (
    <div className="h-screen bg-slate-100 dark:bg-[#0b0d11] text-slate-900 dark:text-slate-100 font-sans selection:bg-brand/30 antialiased flex flex-col overflow-hidden">
      {/* Premium Navbar */}
      <nav className="bg-[#121419] text-white sticky top-0 z-[60] border-b border-white/5 shadow-2xl backdrop-blur-xl bg-opacity-95 shrink-0">
        <div className="max-w-[1920px] mx-auto px-6 h-20 flex items-center justify-between">
          <div className="flex items-center gap-6">
            <button
              onClick={() => setIsSidebarOpen(!isSidebarOpen)}
              className="lg:hidden p-2 hover:bg-white/10 rounded-lg transition-colors"
            >
              {isSidebarOpen ? <X size={20} /> : <Menu size={20} />}
            </button>
            <div className="flex items-center gap-4 group cursor-pointer">
              <div className="w-12 h-12 bg-gradient-to-tr from-brand to-orange-400 rounded-xl flex items-center justify-center shadow-lg shadow-brand/20 group-hover:scale-105 transition-transform duration-300">
                <Trophy className="text-white drop-shadow-md" size={24} strokeWidth={2.5} />
              </div>
              <div className="flex flex-col">
                <span className="text-xl font-black tracking-tighter uppercase leading-none bg-clip-text text-transparent bg-gradient-to-r from-white to-slate-400">SABA BOOK</span>
                <span className="text-[10px] text-brand font-black uppercase tracking-[0.2em] leading-none mt-1.5 opacity-90">Multi-Sport Elite</span>
              </div>
            </div>
          </div>

          <div className="hidden lg:flex items-center gap-10">
            <div className="flex items-center gap-4">
               <div className="p-2 text-slate-500 hover:text-white transition-colors cursor-help">
                  <AlertCircle size={18} />
               </div>
            </div>
          </div>
        </div>
      </nav>

      <div className="flex flex-1 overflow-hidden">
        {/* Modern Sidebar */}
        <aside className={`
          fixed lg:relative top-0 bottom-0 left-0 z-50
          w-72 bg-[#0f1115] border-r border-white/5 transition-transform duration-300 ease-in-out
          ${isSidebarOpen ? 'translate-x-0' : '-translate-x-full lg:hidden'}
          overflow-y-auto shrink-0
        `}>
          <div className="p-6 space-y-8">
            <div>
              <p className="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em] mb-4 px-2">Main Categories</p>
              <div className="space-y-1">
                {sportCategories.map((sport) => (
                  <button
                    key={sport.name}
                    onClick={() => setSelectedSportType(sport.id)}
                    className={`
                      w-full flex items-center justify-between p-3.5 rounded-xl transition-all duration-200 group
                      ${selectedSportType === sport.id
                        ? 'bg-brand text-white shadow-lg shadow-brand/20'
                        : 'text-slate-400 hover:bg-white/5 hover:text-slate-100'}
                    `}
                  >
                    <div className="flex items-center gap-4">
                      <sport.icon size={18} className={selectedSportType === sport.id ? 'text-white' : sport.color || 'text-slate-500 group-hover:text-slate-300'} />
                      <span className="text-xs font-black uppercase tracking-widest">{sport.name}</span>
                    </div>
                  </button>
                ))}
              </div>
            </div>

            <div>
              <p className="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em] mb-4 px-2">Quick Links</p>
              <div className="space-y-1">
                {['Promotions', 'My Bets', 'History', 'Settings'].map((item) => (
                  <div key={item} className="flex items-center justify-between p-3.5 rounded-xl text-slate-500 hover:bg-white/5 hover:text-slate-300 transition-all cursor-not-allowed group">
                    <span className="text-[10px] font-black uppercase tracking-widest">{item}</span>
                    <ChevronRight size={14} className="opacity-0 group-hover:opacity-100 transition-opacity" />
                  </div>
                ))}
              </div>
            </div>
          </div>

        </aside>

        {/* Content Area */}
        <div className="flex-1 overflow-y-auto overflow-x-hidden custom-scrollbar">
          {/* Hero Stats (Integrated) */}
          <div className="bg-[#121419] pt-8 pb-20 px-8 border-b border-white/5 relative z-0">
            <div className="max-w-5xl lg:mx-0 grid grid-cols-1 sm:grid-cols-3 gap-6">
               {[
                 { label: 'Matches Available', value: filteredEvents.length, icon: Activity, color: 'text-brand' },
                 { label: 'Markets Pulsing', value: filteredEvents.reduce((acc, e) => acc + e.markets.length, 0), icon: BarChart3, color: 'text-blue-400' }
               ].map((stat) => (
                 <div key={stat.label} className="bg-white/5 p-5 rounded-2xl border border-white/5 flex items-center gap-5 hover:bg-white/10 transition-all duration-300 shadow-xl">
                    <div className={`p-3 rounded-xl bg-white/5 ${stat.color} shadow-inner`}>
                      <stat.icon size={22} />
                    </div>
                    <div>
                       <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest leading-none mb-2">{stat.label}</p>
                       <p className="text-2xl font-black text-white tabular-nums leading-none tracking-tighter">{stat.value}</p>
                    </div>
                 </div>
               ))}
            </div>
          </div>

          <main className="max-w-5xl px-8 pb-20 -mt-10 relative z-10 mx-0">
            {error ? (
              <div className="bg-red-500 text-white rounded-3xl p-12 text-center shadow-2xl flex flex-col items-center gap-6 animate-slide-in">
                 <div className="p-4 bg-white/20 rounded-full">
                    <AlertCircle size={48} className="animate-pulse" />
                 </div>
                 <div className="space-y-2">
                    <h2 className="text-3xl font-black uppercase tracking-tighter">Transmission Lost</h2>
                    <p className="text-sm font-bold opacity-80 max-w-md mx-auto leading-relaxed">Our data nodes are currently out of sync or under maintenance. Real-time odds calculation is suspended.</p>
                 </div>
                 <button onClick={fetchData} className="mt-4 px-10 py-4 bg-white text-red-600 rounded-2xl font-black text-xs uppercase tracking-widest hover:scale-105 transition-transform shadow-xl">Retry Sync</button>
              </div>
            ) : loading && events.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-40 bg-white/5 backdrop-blur-3xl rounded-[2.5rem] border border-white/5 shadow-2xl">
                 <div className="relative mb-12 w-32 h-32">
                    <div className="absolute inset-0 border-[10px] border-brand/10 rounded-full"></div>
                    <div className="absolute inset-0 border-[10px] border-brand border-t-transparent rounded-full animate-spin"></div>
                    <Trophy size={40} className="absolute inset-0 m-auto text-brand animate-pulse" strokeWidth={3} />
                 </div>
                 <p className="text-xl font-black text-white uppercase tracking-[0.4em] opacity-30 animate-pulse">Initializing Streams</p>
              </div>
            ) : (
              <div className="space-y-8">
                <div className="flex items-center justify-between bg-white dark:bg-dark-surface p-4 rounded-2xl shadow-lux border border-slate-200 dark:border-slate-800 animate-slide-in">
                   <div className="flex items-center gap-4">
                      <div className="w-2 h-8 bg-brand rounded-full"></div>
                      <div>
                        <h2 className="text-lg font-black uppercase tracking-widest text-slate-800 dark:text-white leading-none">
                          {selectedSportType ? (selectedSportType === 1 ? 'Soccer Matches' : 'Basketball Elite') : 'All Active Feeds'}
                        </h2>
                        <p className="text-[10px] font-black text-slate-400 dark:text-slate-500 uppercase tracking-[0.15em] mt-1.5">Real-time Trading Matrix</p>
                      </div>
                   </div>
                </div>

                {filteredEvents.length === 0 ? (
                  <div className="bg-white dark:bg-dark-surface rounded-[2rem] p-40 text-center border-2 border-dashed border-slate-200 dark:border-slate-800 flex flex-col items-center gap-8 shadow-lux opacity-60">
                    <div className="w-24 h-24 bg-slate-100 dark:bg-white/5 rounded-full flex items-center justify-center">
                       <BarChart3 size={40} className="text-slate-300 dark:text-slate-700" />
                    </div>
                    <div className="space-y-3">
                      <p className="text-2xl font-black text-slate-400 dark:text-slate-500 uppercase tracking-[0.2em]">Matrix Empty</p>
                      <p className="text-sm font-bold text-slate-400 opacity-60">No active {selectedSportType === 1 ? 'soccer' : 'basketball'} events found.</p>
                    </div>
                  </div>
                ) : (
                  <div className="grid grid-cols-1 gap-4">
                    {filteredEvents.map((event) => (
                      <div key={event.eventId} className="animate-slide-in">
                        <EventCard event={event} onOpenDetails={openDetails} />
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}
          </main>

          <footer className="bg-transparent py-16 px-8 mt-auto">
            <div className="max-w-5xl flex flex-col md:flex-row justify-between items-center gap-10">
               <div className="flex items-center gap-4">
                  <div className="w-10 h-10 bg-white/5 rounded-lg flex items-center justify-center grayscale opacity-30 hover:grayscale-0 hover:opacity-100 transition-all border border-white/5">
                    <Trophy size={20} className="text-brand" />
                  </div>
                  <p className="text-[9px] font-black text-slate-500 uppercase tracking-[0.25em]">© 2026 SABA ODDSPUSH NETWORK • VERIFIED FEED • NOT FOR DEPLOYMENT</p>
               </div>
               <div className="flex gap-10">
                  {['T&C', 'Integrity', 'Privacy', 'Compliance'].map(link => (
                    <span key={link} className="text-[9px] font-black text-slate-600 dark:text-slate-500 uppercase tracking-widest cursor-pointer hover:text-brand transition-colors">
                      {link}
                    </span>
                  ))}
               </div>
            </div>
          </footer>
        </div>
      </div>

      {/* Global Detail Modal */}
      {selectedEventId && (
        <div className="fixed inset-0 z-[100] preserve-3d flex items-center justify-center p-4 sm:p-6 overflow-hidden">
          <div className="absolute inset-0 bg-slate-900/80 backdrop-blur-md" onClick={() => setSelectedEventId(null)}></div>
          <div className="relative w-full max-w-5xl bg-white dark:bg-[#121419] rounded-[2.5rem] shadow-2xl border border-white/10 overflow-hidden flex flex-col h-[85vh] animate-in fade-in zoom-in duration-300">
            {/* Modal Header */}
            <div className="p-8 border-b border-slate-100 dark:border-slate-800 flex justify-between items-center shrink-0">
              <div className="flex items-center gap-6">
                <div className="w-16 h-16 bg-brand/10 rounded-2xl flex items-center justify-center">
                  <Shield size={32} className="text-brand" />
                </div>
                <div>
                  <h3 className="text-2xl font-black text-slate-900 dark:text-white uppercase tracking-tighter leading-none mb-2">
                    {detailedEvent ? `${detailedEvent.homeTeamName} vs ${detailedEvent.awayTeamName}` : 'Syncing Data...'}
                  </h3>
                  <p className="text-xs font-black text-brand uppercase tracking-[0.2em]">
                    {detailedEvent?.leagueName || 'Real-time Matrix'}
                  </p>
                </div>
              </div>
              <button
                onClick={() => setSelectedEventId(null)}
                className="p-3 hover:bg-slate-100 dark:hover:bg-white/10 rounded-2xl transition-all text-slate-400 hover:text-slate-900 dark:hover:text-white"
              >
                <X size={28} />
              </button>
            </div>

            {/* Modal Content */}
            <div className="flex-1 overflow-y-auto p-8 custom-scrollbar bg-slate-50 dark:bg-black/20 min-h-0">
              {loadingModal ? (
                <div className="flex flex-col items-center justify-center h-full min-h-[400px] gap-6">
                  <div className="relative w-20 h-20">
                    <div className="absolute inset-0 border-4 border-brand/20 rounded-full"></div>
                    <div className="absolute inset-0 border-4 border-brand border-t-transparent rounded-full animate-spin"></div>
                  </div>
                  <p className="text-xs font-black text-slate-400 uppercase tracking-[0.4em] animate-pulse">Initializing Streams</p>
                </div>
              ) : detailedEvent ? (
                <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                  {detailedEvent.markets.map(market => (
                    <div key={market.marketId} className="bg-white dark:bg-[#1a1d23] p-6 rounded-[1.5rem] border border-slate-100 dark:border-slate-800 shadow-xl hover:border-brand/30 transition-colors">
                      <MarketOdds market={market} />
                    </div>
                  ))}
                </div>
              ) : (
                <div className="flex items-center justify-center h-full min-h-[400px]">
                  <p className="text-slate-500 font-bold uppercase tracking-widest">Failed to initialize data feed.</p>
                </div>
              )}
            </div>

            <div className="p-6 bg-slate-100/50 dark:bg-slate-900/50 border-t border-slate-100 dark:border-slate-800 flex justify-between items-center px-10 shrink-0">
              <span className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">SABA ODDSPUSH NETWORK • VERIFIED FEED</span>
              <span className="text-[10px] font-black text-brand uppercase tracking-[0.2em]">ID: {selectedEventId}</span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default App;
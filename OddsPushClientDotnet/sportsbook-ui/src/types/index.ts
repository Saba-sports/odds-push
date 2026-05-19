export interface Selection {
  selectionKey: string;
  price: number;
  point?: number;
}

export interface Market {
  marketId: number;
  betType: number;
  betTypeName: string;
  marketStatus: string;
  selections: Selection[];
}

export interface SportEvent {
  eventId: number;
  sportType: number;
  leagueName: string;
  homeTeamName: string;
  awayTeamName: string;
  kickoffTime: string;
  eventStatus: string;
  liveHomeScore: number;
  liveAwayScore: number;
  currentPeriod?: string;
  isLive: boolean;
  markets: Market[];
}

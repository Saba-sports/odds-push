import axios from 'axios';
import type { SportEvent } from './types/index';

const API_BASE_URL = 'http://localhost:5078/api'; // Standard .NET port, update if needed

export const getEvents = async (sportType?: number | null, eventStatus?: string): Promise<SportEvent[]> => {
  const response = await axios.get(`${API_BASE_URL}/events`, {
    params: { sportType, eventStatus }
  });
  return response.data;
};

export const getEvent = async (id: number): Promise<SportEvent> => {
  const response = await axios.get(`${API_BASE_URL}/events/${id}`);
  return response.data;
};

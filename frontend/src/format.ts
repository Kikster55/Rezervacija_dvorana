import type { ResourceType, ReservationStatus } from './types';

/** Backend šalje lokalno vrijeme bez vremenske zone ("2026-09-22T10:00:00").
 *  Zato formatiramo izravno iz teksta - bez Date objekta nema ni pomaka u satima. */

export const formatDate = (iso: string): string => {
  const [year, month, day] = iso.slice(0, 10).split('-');
  return `${day}.${month}.${year}.`;
};

export const formatTime = (iso: string): string => iso.slice(11, 16);

export const formatDateTime = (iso: string): string => `${formatDate(iso)} u ${formatTime(iso)}`;

/** Današnji datum u obliku koji traži <input type="date"> i API. */
export const todayInputValue = (): string => {
  const now = new Date();
  const month = `${now.getMonth() + 1}`.padStart(2, '0');
  const day = `${now.getDate()}`.padStart(2, '0');
  return `${now.getFullYear()}-${month}-${day}`;
};

export const resourceTypeLabel = (type: ResourceType): string =>
  type === 'MeetingRoom' ? 'Sala' : 'Oprema';

export const statusLabel = (status: ReservationStatus): string =>
  status === 'Active' ? 'Aktivna' : 'Otkazana';

/** "08:00:00" -> "08:00" */
export const shortTime = (time: string): string => time.slice(0, 5);

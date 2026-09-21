export type UserRole = 'User' | 'Admin';

export interface User {
  id: number;
  email: string;
  fullName: string;
  role: UserRole;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export type ResourceType = 'MeetingRoom' | 'Equipment';

export interface Resource {
  id: number;
  name: string;
  type: ResourceType;
  location: string;
  capacity: number;
  description: string | null;
  /** Oblik "08:00:00" */
  openingTime: string;
  closingTime: string;
  slotMinutes: number;
  isActive: boolean;
}

export interface TimeSlot {
  /** Oblik "2026-09-22T10:00:00" - lokalno vrijeme, bez vremenske zone. */
  start: string;
  end: string;
  isAvailable: boolean;
}

export interface ResourceAvailability {
  resourceId: number;
  resourceName: string;
  date: string;
  slots: TimeSlot[];
}

export type ReservationStatus = 'Active' | 'Cancelled';

export interface Reservation {
  id: number;
  resourceId: number;
  resourceName: string;
  userId: number;
  userFullName: string;
  startsAt: string;
  endsAt: string;
  note: string | null;
  status: ReservationStatus;
  createdAt: string;
  cancelledAt: string | null;
}

export interface ReservationStats {
  total: number;
  active: number;
  cancelled: number;
  mostUsedResource: string | null;
  hoursBooked: number;
}

export interface ResourceInput {
  name: string;
  type: ResourceType;
  location: string;
  capacity: number;
  description: string | null;
  openingTime: string;
  closingTime: string;
  slotMinutes: number;
  isActive: boolean;
}

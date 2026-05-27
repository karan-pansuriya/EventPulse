import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SeatUpdate {
  eventId: number;
  remainingSeats: number;
}

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private hubConnection: signalR.HubConnection | null = null;
  private connectionPromise: Promise<void> | null = null;
  private joinedGroups = new Set<number>();

  private seatUpdateSubject = new Subject<SeatUpdate>();
  private connectionStateSubject = new BehaviorSubject<boolean>(false);

  seatUpdates$: Observable<SeatUpdate> = this.seatUpdateSubject.asObservable();
  isConnected$: Observable<boolean> = this.connectionStateSubject.asObservable();

  private baseUrl = environment.apiUrl.replace('/api', '');

  async startConnection(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    if (this.connectionPromise) {
      return this.connectionPromise;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.baseUrl}/hubs/seats`)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    this.hubConnection.on('SeatUpdated', (data: SeatUpdate) => {
      this.seatUpdateSubject.next(data);
    });

    this.hubConnection.onreconnecting(() => {
      this.connectionStateSubject.next(false);
    });

    this.hubConnection.onreconnected(async () => {
      this.connectionStateSubject.next(true);
      await this.rejoinGroups();
    });

    this.hubConnection.onclose(async () => {
      this.connectionStateSubject.next(false);
      this.connectionPromise = null;
    });

    this.connectionPromise = this.hubConnection.start()
      .then(() => {
        this.connectionStateSubject.next(true);
      })
      .catch(() => {
        this.connectionPromise = null;
      });

    return this.connectionPromise;
  }

  async joinEventGroup(eventId: number): Promise<void> {
    this.joinedGroups.add(eventId);

    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
      await this.startConnection();
    }

    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('JoinEventGroup', eventId);
      } catch {
        // ignore
      }
    }
  }

  async leaveEventGroup(eventId: number): Promise<void> {
    this.joinedGroups.delete(eventId);

    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('LeaveEventGroup', eventId);
      } catch {
        // ignore
      }
    }
  }

  private async rejoinGroups(): Promise<void> {
    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) return;

    for (const eventId of this.joinedGroups) {
      try {
        await this.hubConnection.invoke('JoinEventGroup', eventId);
      } catch {
        // ignore
      }
    }
  }

  ngOnDestroy(): void {
    this.hubConnection?.stop();
    this.seatUpdateSubject.complete();
    this.connectionStateSubject.complete();
  }
}

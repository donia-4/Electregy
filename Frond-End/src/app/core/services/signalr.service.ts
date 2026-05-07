import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { BehaviorSubject, Observable } from 'rxjs';

export interface PowerAlert {
  deviceName: string;
  message: string;
  deviceType?: string;
  wattsConsumed?: number;
  timestamp?: string;
  isAbnormal?: boolean;
  addedAt?: string;
}

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection!: signalR.HubConnection;
  private alertSubject = new BehaviorSubject<PowerAlert[]>([]);
  
  public alerts$: Observable<PowerAlert[]> = this.alertSubject.asObservable();
  public unreadCount$ = new BehaviorSubject<number>(0);

  constructor() { }

  public startConnection(token: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://peakwise.runasp.net/consumption', {
        accessTokenFactory: () => token,
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    this.start();
  }

  private start(): void {
    this.hubConnection.start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.error('Error while starting connection: ' + err));

    this.hubConnection.on('ReceivePowerAlert', (data: PowerAlert) => {
      console.log('New Alert Received:', data);
      
      const currentAlerts = this.alertSubject.value;
      this.alertSubject.next([data, ...currentAlerts]);
      
      this.unreadCount$.next(this.unreadCount$.value + 1);
    });
  }

  public resetUnreadCount() {
    this.unreadCount$.next(0);
  }
}
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ChatService {

  private hubConnection!: signalR.HubConnection;
  private isConnected = false;

  startConnection(): Promise<void> {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://peakwise.runasp.net/chatbot', {
        accessTokenFactory: () => localStorage.getItem('accessToken') || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.onclose(() => {
      this.isConnected = false;
    });

    this.hubConnection.onreconnected(() => {
      this.isConnected = true;
    });

    return this.hubConnection.start().then(() => {
      this.isConnected = true;
      console.log('SignalR Connected');
    }).catch(err => {
      console.error('SignalR Connection Error:', err);
    });
  }

  onReceiveMessage(callback: (msg: string) => void) {
    this.hubConnection.on('receivefromchatbot', (message: string) => {
      console.log('Bot:', message);
      callback(message);
    });
  }

  sendMessage(message: string) {
    if (!this.isConnected) {
      console.error('SignalR not connected yet');
      return Promise.reject('Not connected');
    }

    return this.hubConnection.invoke('sendmessagetogemeni', message);
  }
}
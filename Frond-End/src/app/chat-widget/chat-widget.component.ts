import { Component, OnInit, ViewChildren, QueryList, ElementRef, AfterViewChecked, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatService } from '../core/services/chat.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-chat-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-widget.component.html',
  styleUrls: ['./chat-widget.component.scss']
})
export class ChatWidgetComponent implements OnInit, AfterViewChecked {

  @ViewChildren('chatBodyMini') chatBodyMini!: QueryList<ElementRef>;
  @ViewChildren('chatBodyFull') chatBodyFull!: QueryList<ElementRef>;

  isOpen = false;
  isFullscreen = false;

  messages: { text: string, sender: 'user' | 'bot' }[] = [];
  userInput = '';
  
  isBotTyping = false; 

  constructor(
    private chatService: ChatService, 
    private zone: NgZone 
  ) {}

  ngOnInit() {
    this.chatService.startConnection()
      .then(() => console.log('Connected OK'))
      .catch(err => console.log(err));

    this.chatService.onReceiveMessage((msg) => {
      console.log("Received in UI:", msg); 
      
      this.zone.run(() => {
        this.isBotTyping = false; 
        this.messages.push({ text: msg, sender: 'bot' });
      });
    });
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom(): void {
    try {
      if (this.chatBodyMini && this.chatBodyMini.first) {
        this.chatBodyMini.first.nativeElement.scrollTop = this.chatBodyMini.first.nativeElement.scrollHeight;
      }
      if (this.chatBodyFull && this.chatBodyFull.first) {
        this.chatBodyFull.first.nativeElement.scrollTop = this.chatBodyFull.first.nativeElement.scrollHeight;
      }
    } catch(err) { }
  }

  send() {
    if (!this.userInput.trim()) return;

    const msg = this.userInput;

    this.messages.push({ text: msg, sender: 'user' });
    
    this.isBotTyping = true;

    this.chatService.sendMessage(msg)
      .then(() => console.log('Message sent'))
      .catch(err => {
        console.error('Send error:', err);
        this.zone.run(() => this.isBotTyping = false); 
      });

    this.userInput = '';
  }

  toggleChat() {
    this.isOpen = !this.isOpen;
    if (this.isOpen) this.isFullscreen = false;
  }

  openFullscreen() {
    this.isFullscreen = true;
    this.isOpen = false;
  }

  closeFullscreen() {
    this.isFullscreen = false;
  }
}
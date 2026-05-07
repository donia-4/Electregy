import { Component, OnInit, OnDestroy } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ChatWidgetComponent } from '../chat-widget/chat-widget.component';
import { filter } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { AuthService } from '../core/services/auth.service';
import { UserService } from '../core/services/user.service';
import { SignalRService, PowerAlert } from '../core/services/signalr.service'; 
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [
    RouterLink,       
    RouterLinkActive, 
    RouterOutlet,
    ChatWidgetComponent,
    CommonModule,
  ],
  templateUrl: './dashboard-layout.component.html',
  styleUrls: ['./dashboard-layout.component.scss']
})
export class DashboardLayoutComponent implements OnInit, OnDestroy {
  isDevicesPage = false;
  isLogoutModalOpen = false; 
  userName: string = '';
  pageTitle: string = 'Dashboard Overview';
  
  notifications: PowerAlert[] = [];
  unreadCount: number = 0;
  isNotifDropdownOpen = false;
  private sub: Subscription = new Subscription();

  constructor(
     private router: Router,
     private authService: AuthService,
     private userService: UserService,
     private signalRService: SignalRService  
) {}

ngOnInit() {
  this.userName = localStorage.getItem('userName') || 'User';
  const token = localStorage.getItem('accessToken'); 
  
  if (token) {
    this.signalRService.startConnection(token);
    
    this.sub.add(
      this.signalRService.alerts$.subscribe(alerts => {
        this.notifications = alerts;
      })
    );

    this.sub.add(
      this.signalRService.unreadCount$.subscribe(count => {
        this.unreadCount = count;
      })
    );
  }

  this.router.events.pipe(
    filter(event => event instanceof NavigationEnd)
  ).subscribe(() => {
    this.pageTitle = this.getPageTitle(this.router.routerState.root);
    this.isDevicesPage = this.router.url.includes('/devices');
  });
}

getPageTitle(route: any): string {
  while (route.firstChild) { route = route.firstChild; }
  return route.snapshot.data['title'] || 'Dashboard Overview';
}

toggleNotifications() {
  this.isNotifDropdownOpen = !this.isNotifDropdownOpen;
  if (this.isNotifDropdownOpen) {
    this.signalRService.resetUnreadCount(); 
  }
}

closeNotifications() {
  setTimeout(() => { this.isNotifDropdownOpen = false; }, 200);
}

openLogoutModal() { this.isLogoutModalOpen = true; }
closeLogoutModal() { this.isLogoutModalOpen = false; }

confirmLogout() {
  this.authService.logout().subscribe({
    next: () => {
      localStorage.clear();
      this.closeLogoutModal();
      this.router.navigate(['/login']);
    },
    error: () => {
      localStorage.clear();
      this.closeLogoutModal();
      this.router.navigate(['/login'], { replaceUrl: true });
    }
  });
}

ngOnDestroy() {
  this.sub.unsubscribe();
}
}
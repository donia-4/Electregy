import { Routes } from '@angular/router';
import { LandingComponent } from './landing/landing.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { DashboardLayoutComponent } from './dashboard-layout/dashboard-layout.component';
import { DashboardHomeComponent } from './dashboard-home/dashboard-home.component'; 

export const routes: Routes = [
  { path: '', component: LandingComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  
  { 
    path: 'dashboard', 
    component: DashboardLayoutComponent,
    children: [
      { 
        path: '', 
        component: DashboardHomeComponent,
        data: { title: 'Dashboard Overview' } 
      },
      
      { 
        path: 'devices', 
        loadComponent: () => import('./devices/devices.component').then(m => m.DevicesComponent),
        data: { title: 'Devices Management' } 
      },
      
      { 
        path: 'simulator', 
        loadComponent: () => import('./simulator/simulator.component').then(m => m.SimulatorComponent),
        data: { title: 'Energy Simulator' } 
      },
    ]
  },
  
  { path: '**', redirectTo: '' }
];
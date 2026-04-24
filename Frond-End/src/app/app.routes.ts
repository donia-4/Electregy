import { Routes } from '@angular/router';
import { LandingComponent } from './landing/landing.component';
import { RegisterComponent } from './register/register.component';

import { LoginComponent } from './login/login.component';
export const routes: Routes = [
      { path: '', component: LandingComponent },
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent }, 
      { path: '', redirectTo: '/login', pathMatch: 'full' },

];

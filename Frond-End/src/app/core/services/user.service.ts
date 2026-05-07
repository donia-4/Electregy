import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class UserService {

  getUserFromToken() {
    const token = localStorage.getItem('accessToken');
    if (!token) return null;

    const payload = JSON.parse(atob(token.split('.')[1]));

    return {
      name: payload.given_name || payload.email,
      email: payload.email,
      role: payload.role
    };
  }
  getUserName() {
  return localStorage.getItem('userName');
}
}
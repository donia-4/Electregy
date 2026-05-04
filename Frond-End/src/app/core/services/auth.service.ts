import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RegisterRequest, RegisterResponse } from '../models/register.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

private baseUrl = 'https://peakwise.runasp.net/api/Account';

  constructor(private http: HttpClient) {}

  register(data: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.baseUrl}/register`, data);
  }

  login(data: { email: string; password: string }) {
  return this.http.post<any>(
    'https://peakwise.runasp.net/api/Account/login',
    data
  );
}

logout() {
  return this.http.post(
    'https://peakwise.runasp.net/api/Account/logout',
    {}
  );
}
}
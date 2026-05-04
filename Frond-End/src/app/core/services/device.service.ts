import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class DeviceService {

  private baseUrl = 'https://peakwise.runasp.net/api/Device';

  constructor(private http: HttpClient) {}

getDevices(pageNumber = 1, pageSize = 10) {
    return this.http.get(this.baseUrl + `?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }
  
addDevice(data: FormData) {
  return this.http.post(this.baseUrl, data);
}

updateDevice(data: FormData) {
  return this.http.put(this.baseUrl, data);
}

deleteDevice(id: number) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }


}
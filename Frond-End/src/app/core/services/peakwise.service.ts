import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
@Injectable({ providedIn: 'root' })
export class PeakWiseService {

  private baseUrl = 'https://peakwise.runasp.net/api/PeakWiseEnergyAI';

  constructor(private http: HttpClient) {}

 simulate(payload: any) {
  return this.http.post(
    'https://peakwise.runasp.net/api/PeakWiseEnergyAI/simulate',
    payload
  );
}

energyPlan(payload: any) {
  return this.http.post(
    'https://peakwise.runasp.net/api/PeakWiseEnergyAI/energy-plan',
    payload
  );
}

controlActions(payload: any) {
  return this.http.post(
    'https://peakwise.runasp.net/api/PeakWiseEnergyAI/control-actions',
    payload
  );
}


getConsumptionSummary() {
  return this.http.get<any>(
    'https://peakwise.runasp.net/api/Consumption/summary'
  );
}

 getAiRecommendation(): Observable<any> {
    return this.http.post<any>(
      'https://peakwise.runasp.net/api/SmartAssistant/recommand',
      {} 
    );
  }

    syncChartData(): Observable<any> {
    return this.http.post(
      'https://peakwise.runasp.net/api/Consumption/sync-my-chart', 
      {}
    );
  }

  getChartData(): Observable<any> {
    return this.http.get(
      'https://peakwise.runasp.net/api/Consumption/chart-data'
    );
  }
}
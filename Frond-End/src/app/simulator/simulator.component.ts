import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PeakWiseService } from '../core/services/peakwise.service';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-simulator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './simulator.component.html',
  styleUrls: ['./simulator.component.scss']
})
export class SimulatorComponent {

  result: any = null;
  planResult: any = null;
  actionsResult: any = null;

  loading = false;
  bestAction: string = '';
bestSaving: number = 0;
scenarios: any[] = [];

  temp = 22;
  occupancy = 50;
  orders = 30;
  isWeekend = false;
  selectedPeriod = 3;

  periods = [
    { value: 0, label: 'Opening' },
    { value: 1, label: 'Quiet Noon' },
    { value: 2, label: 'Pre-Peak' },
    { value: 3, label: 'The Peak' },
    { value: 4, label: 'Closing' }
  ];

  constructor(
    private peakWiseService: PeakWiseService,
    private cd: ChangeDetectorRef
  ) {}

simulate() {

  this.loading = true;

  const periodLabel =
    this.periods.find(p => p.value === this.selectedPeriod)?.label;

  const basePayload = {
    temp: this.temp,
    occupancy: this.occupancy,
    orders: this.orders,
    is_weekend: this.isWeekend ? 1 : 0,
    period: periodLabel
  };

  const simulatePayload = {
    periods: [basePayload],
    reduce_Occupancy_Pct: 0,
    reduce_Orders_Pct: 0
  };

  this.result = null;
  this.planResult = null;
  this.actionsResult = null;

  this.peakWiseService.simulate(simulatePayload).subscribe({
    next: (res: any) => {
      this.result = res.data;

      this.loadDetails(basePayload);
    },
    complete: () => {
      this.loading = false;
      this.cd.detectChanges(); 
    }
  });
}

loadDetails(payload: any) {

  this.peakWiseService.energyPlan(payload).subscribe({
    next: (res: any) => {
      this.planResult = res.data;
      this.cd.detectChanges();
    }
  });

  this.peakWiseService.controlActions(payload).subscribe({
    next: (res: any) => {
      this.actionsResult = res.data;
      this.cd.detectChanges(); 
    }
  });
}
}


// simulate() {
//   this.loading = true;

//   const selectedPeriodObj = this.periods.find(
//     p => p.value === Number(this.selectedPeriod)
//   );

//   const payload = {
//     periods: [
//       {
//         temp: this.temp,
//         occupancy: this.occupancy,
//         orders: this.orders,
//         is_weekend: this.isWeekend ? 1 : 0,
//         period: selectedPeriodObj?.label ?? 'The Peak'
//       }
//     ],
//     reduce_Occupancy_Pct: 0,
//     reduce_Orders_Pct: 0
//   };

//   console.log('PAYLOAD:', payload);

//   this.peakWiseService.simulate(payload).subscribe({
//     next: (res: any) => {

//       this.result = null;

//       setTimeout(() => {
//         this.result = res.data;
//         this.loading = false;
//         this.cd.detectChanges();
//       });

//     },
//     error: () => {
//       this.loading = false;
//       this.cd.detectChanges();
//     }
//   });
// }
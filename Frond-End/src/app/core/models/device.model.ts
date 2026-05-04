export interface Device {
  id: number;
  name: string;
  type: string;
  watts: number;
  hoursPerDay: number;
  estimatedMonthlyCostEGP: number;
}
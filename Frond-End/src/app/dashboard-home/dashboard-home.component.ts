import { Component, AfterViewInit, ViewChild, ElementRef, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import Chart from 'chart.js/auto';
import { PeakWiseService } from '../core/services/peakwise.service';
import { CommonModule } from '@angular/common';
import { Subscription, interval, startWith } from 'rxjs';
import { SignalRService, PowerAlert } from '../core/services/signalr.service'; 

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-home.component.html',
  styleUrls: ['./dashboard-home.component.scss']
})
export class DashboardHomeComponent implements OnInit, AfterViewInit, OnDestroy {
  
  @ViewChild('chartCanvas') chartRef!: ElementRef<HTMLCanvasElement>;
  
  summary: any = null;
  aiMessage: string = '';
  loadingSummary = true;
  isChartLoading = false;
  
  activeAlert: PowerAlert | null = null;
  
  private chart: Chart | undefined;
  private autoRefreshSubscription: Subscription | undefined;
  private signalRSubscription: Subscription | undefined;

  constructor(
    private peakWiseService: PeakWiseService,
    private cdRef: ChangeDetectorRef,
    private signalRService: SignalRService
  ) {}

  ngOnInit() {
    this.loadDashboardData();
    this.startAutoRefreshForCards();
    this.listenForLiveAlerts(); 
  }

  ngAfterViewInit() {
    this.initEmptyChart();
    this.updateChartWithRealData();
  }

  ngOnDestroy() {
    if (this.autoRefreshSubscription) this.autoRefreshSubscription.unsubscribe();
    if (this.signalRSubscription) this.signalRSubscription.unsubscribe();
    if (this.chart) this.chart.destroy();
  }

  listenForLiveAlerts() {
    this.signalRSubscription = this.signalRService.alerts$.subscribe(alerts => {
      if (alerts && alerts.length > 0) {
        this.activeAlert = alerts[0]; 
        this.cdRef.markForCheck();
      } else {
        this.activeAlert = null; 
        this.cdRef.markForCheck();
      }
    });
  }

  loadDashboardData() {
    this.loadingSummary = true;
    
    this.peakWiseService.getConsumptionSummary().subscribe({
      next: (res: any) => {
        if (res && res.data) this.summary = res.data;
        else this.summary = res;
        this.loadingSummary = false;
        this.cdRef.markForCheck();
      },
      error: (err) => {
        console.error(err);
        this.loadingSummary = false;
        this.cdRef.markForCheck();
      }
    });

    this.peakWiseService.getAiRecommendation().subscribe({
      next: (res: any) => {
        this.aiMessage = res.response || "There are no recommendations at the moment";
        this.cdRef.markForCheck();
      },
      error: (err) => {
        console.error(err);
        this.cdRef.markForCheck();
      }
    });
  }

  updateChartWithRealData() {
    setTimeout(() => {
      this.isChartLoading = true;
      this.cdRef.markForCheck();
    }, 0);

    this.peakWiseService.syncChartData().subscribe({
      next: () => {
        this.peakWiseService.getChartData().subscribe({
          next: (res: any) => {
            const rawData = res.data; 
            
            const labels = rawData.map((item: any) => item.time);
            const usageData = rawData.map((item: any) => item.usage);
            const costData = rawData.map((item: any) => item.cost);

            this.renderChart(labels, usageData, costData);
            
            setTimeout(() => {
              this.isChartLoading = false;
              this.cdRef.markForCheck();
            }, 0);
          },
          error: (err) => {
            console.error("Error fetching chart ", err);
            setTimeout(() => {
              this.isChartLoading = false;
              this.cdRef.markForCheck();
            }, 0);
          }
        });
      },
      error: (err) => {
        console.error("Error syncing chart:", err);
        setTimeout(() => {
          this.isChartLoading = false;
          this.cdRef.markForCheck();
        }, 0);
      }
    });
  }

    renderChart(labels: string[], usageData: number[], costData: number[]) {
    if (!this.chartRef || !this.chartRef.nativeElement) return;
    const ctx = this.chartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    if (this.chart) this.chart.destroy();

    const gradient = ctx.createLinearGradient(0, 0, 0, 300);
    gradient.addColorStop(0, 'rgba(0,195,255,0.4)');
    gradient.addColorStop(1, 'rgba(0,195,255,0)');

    this.chart = new Chart(ctx, {
      type: 'line',
       data:{
        labels: labels,
        datasets: [{
          label: 'Usage (kW)',
          data: usageData,
          borderColor: '#00c3ff',
          backgroundColor: gradient,
          fill: true,
          tension: 0.4,
          pointRadius: 4,
          pointBackgroundColor: '#fff',
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              afterLabel: function(context) {
                const cost = costData[context.dataIndex];
                return cost ? 'Cost: ' + cost + ' EGP' : '';
              }
            }
          }
        },
        scales: {
          x: { grid: { display: false }, ticks: { color: '#888' } },
          y: { grid: { color: 'rgba(255,255,255,0.05)' }, ticks: { color: '#888' } }
        }
      }
    });
  }

  initEmptyChart() { }

  startAutoRefreshForCards() {
    this.autoRefreshSubscription = interval(60000)
      .pipe(startWith(0))
      .subscribe(() => {
        this.loadDashboardData();
      });
  }
}
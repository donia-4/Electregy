import { Component, OnInit, ChangeDetectorRef } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DeviceService } from '../core/services/device.service';
import { Device } from '../core/models/device.model';

@Component({
  selector: 'app-devices',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './devices.component.html',
  styleUrls: ['./devices.component.scss']
})
export class DevicesComponent implements OnInit {

  devices: Device[] = [];
  
  isAddModalOpen = false;
  isEditModalOpen = false;
  isDeleteModalOpen = false;

  selectedDeviceForEdit: Device | null = null;
  deviceToDelete: Device | null = null;

  newDevice: any = {
    name: '',
    type: '',
    watts: 0,
    hoursPerDay: 0
  };

  pageNumber = 1;
  pageSize = 10;
  totalPages = 1;
  hasNextPage = false;
  hasPreviousPage = false;

  constructor(
    private deviceService: DeviceService,
    private cdRef: ChangeDetectorRef 
  ) {}

  ngOnInit() {
    this.loadDevices();
  }

  loadDevices() {
    this.deviceService.getDevices(this.pageNumber, this.pageSize).subscribe({
      next: (res: any) => {
        const data = res?.data;
        
        this.devices = data?.items ?? [];
        this.pageNumber = data?.pageNumber ?? 1;
        this.totalPages = data?.totalPages ?? 1;
        this.hasNextPage = data?.hasNextPage ?? false;
        this.hasPreviousPage = data?.hasPreviousPage ?? false;
        this.cdRef.detectChanges();
      },
      error: (err) => {
        console.error("Load Error:", err);
      }
    });
  }

  nextPage() {
    if (this.hasNextPage) {
      this.pageNumber++;
      this.loadDevices();
    }
  }

  prevPage() {
    if (this.hasPreviousPage) {
      this.pageNumber--;
      this.loadDevices();
    }
  }

  openAddModal() {
    this.newDevice = { name: '', type: '', watts: 0, hoursPerDay: 0 };
    this.isAddModalOpen = true;
  }

  closeAddModal() {
    this.isAddModalOpen = false;
  }

  addNewDevice() {
    if (!this.newDevice.name || !this.newDevice.type) {
      alert('Please fill Name and Type');
      return;
    }

    const formData = new FormData();
    formData.append('Name', this.newDevice.name);
    formData.append('Type', this.newDevice.type);
    formData.append('Watts', String(this.newDevice.watts));
    formData.append('HoursPerDay', String(this.newDevice.hoursPerDay));

    this.deviceService.addDevice(formData).subscribe({
      next: () => {
        this.isAddModalOpen = false;
        this.cdRef.detectChanges();
        this.pageNumber = 1;
        this.loadDevices();
      },
      error: (err) => {
        console.error("Add Error", err);
        alert("Failed to add device");
      }
    });
  }

  openEditModal(device: Device) {
    this.selectedDeviceForEdit = { ...device };
    this.isEditModalOpen = true;
  }

  closeEditModal() {
    this.isEditModalOpen = false;
    this.selectedDeviceForEdit = null;
  }

  saveEditChanges() {
    if (!this.selectedDeviceForEdit) return;

    const formData = new FormData();
    formData.append('Id', String(this.selectedDeviceForEdit.id));
    formData.append('Name', this.selectedDeviceForEdit.name);
    formData.append('Type', this.selectedDeviceForEdit.type);
    formData.append('Watts', String(this.selectedDeviceForEdit.watts));
    formData.append('HoursPerDay', String(this.selectedDeviceForEdit.hoursPerDay));

    this.deviceService.updateDevice(formData).subscribe({
      next: () => {
        this.isEditModalOpen = false;
        this.cdRef.detectChanges();
        this.loadDevices();
      },
      error: (err) => console.error("Update Error", err)
    });
  }

  openDeleteModal(device: Device) {
    this.deviceToDelete = device;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal() {
    this.isDeleteModalOpen = false;
    this.deviceToDelete = null;
  }

  confirmDelete() {
    if (!this.deviceToDelete) return;

    this.deviceService.deleteDevice(this.deviceToDelete.id).subscribe({
      next: () => {
        this.isDeleteModalOpen = false;
        this.cdRef.detectChanges();
        this.deviceToDelete = null;
        this.loadDevices();
      },
      error: (err) => console.error("Delete Error", err)
    });
  }
}
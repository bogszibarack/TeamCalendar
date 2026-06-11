import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DataService } from './services/data';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  title = 'Team Leave & On-Call Calendar';

  team: any = [];
  leaves: any = [];
  onCallSchedule: any = [];

  newLeave = {
    teamMemberId: 0,
    startDate: '',
    endDate: '',
    reason: '',
    status: 0
  };

  errorMessage: string = '';
  successMessage: string = '';

  constructor(private dataService: DataService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadAllData();
  }

  loadAllData(): void {
    this.dataService.getTeam().subscribe({
      next: (data) => {
        this.team = data;
        this.cdr.markForCheck();
      },
      error: () => this.errorMessage = 'Failed to load team members. Is the backend running?'
    });
    
    this.dataService.getLeaves().subscribe({
      next: (data) => {
        this.leaves = data;
        this.cdr.markForCheck();
      },
      error: () => this.errorMessage = 'Failed to load leave requests.'
    });
    
    this.dataService.getOnCallSchedule().subscribe({
      next: (data) => {
        this.onCallSchedule = data;
        this.cdr.markForCheck();
      },
      error: () => this.errorMessage = 'Failed to load on-call schedule.'
    });
  }

  submitLeave(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.newLeave.teamMemberId || !this.newLeave.startDate || !this.newLeave.endDate || !this.newLeave.reason) {
      this.errorMessage = 'All fields are required!';
      this.cdr.markForCheck();
      return;
    }

    this.newLeave.teamMemberId = Number(this.newLeave.teamMemberId);

    this.dataService.createLeave(this.newLeave).subscribe({
      next: () => {
        this.successMessage = 'Leave request successfully submitted!';
        this.loadAllData();
        this.newLeave = { teamMemberId: 0, startDate: '', endDate: '', reason: '', status: 0 };
        this.cdr.markForCheck();
      },
      error: (err) => {
        if (err && err.error && typeof err.error === 'string') {
          this.errorMessage = err.error; // backendtől kapott hiba
        } else {
          this.errorMessage = 'An error occurred while saving! (e.g. invalid date range or conflict)';
        }
        this.cdr.markForCheck();
      }
    });
  }

  changeStatus(leaveId: number, status: number): void {
    this.dataService.updateLeaveStatus(leaveId, status).subscribe({
      next: () => {
        this.loadAllData();
      },
      error: () => {
        this.errorMessage = 'Failed to update leave request status.';
        this.cdr.markForCheck();
      }
    });
  }

  getStatusLabel(status: number): string {
    switch(status) {
      case 0: return 'Pending';
      case 1: return 'Approved';
      case 2: return 'Rejected';
      default: return 'Unknown';
    }
  }

  // Kérés ütközik-e az adott személy BÁRMELYIK On-Call hetével
  hasOnCallConflict(leave: any): boolean {
    if (!leave || !leave.teamMemberId) return false;

    // Átalakítjuk a kérés dátumait összehasonlítható formátumra
    const leaveStart = new Date(leave.startDate);
    const leaveEnd = new Date(leave.endDate);

    // Check összes On-Call
    for (const slot of this.onCallSchedule) {
      // Ha ez a hét pont azé a személyé, akinek a szabadságát nézzük
      if (slot.teamMemberId === leave.teamMemberId) {
        const slotStart = new Date(slot.weekStart);
        const slotEnd = new Date(slot.weekEnd);

        // Van-e átfedés a szabadság és az ügyeleti hét között
        if (leaveStart <= slotEnd && leaveEnd >= slotStart) {
          return true; 
        }
      }
    }
    return false; 
  }

}
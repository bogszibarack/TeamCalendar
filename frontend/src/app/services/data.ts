import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DataService { // (Service) felelős a .NET backend REST API-jával való teljes kommunikációért és a HTTP kérések kezeléséért
  private apiUrl = 'http://localhost:5259/api';

  constructor(private http: HttpClient) { }

  getTeam(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/team`);
  }

  getLeaves(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/leave`);
  }

  createLeave(leave: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/leave`, leave);
  }

  updateLeaveStatus(id: number, status: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/leave/${id}/status?newStatus=${status}`, {});
  }

  getOnCallSchedule(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/oncall`);
  }
}
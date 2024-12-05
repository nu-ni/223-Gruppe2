import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class BookingService {
  private apiUrl = 'http://localhost:5000/api/v1/bookings';

  constructor(private http: HttpClient) {}

  book(SourceId: string, DestinationId: string, Amount: number): Observable<any> {
    const bookingData = {
      SourceId: parseInt(SourceId, 10),
      DestinationId: parseInt(DestinationId, 10), 
      Amount,
    };
    return this.http.post(this.apiUrl, bookingData);
  }

  getBookingHistory(): Observable<any[]> {
    const historyUrl = `${this.apiUrl}/history`;
    return this.http.get<any[]>(historyUrl);
  }
}

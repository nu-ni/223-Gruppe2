import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BookingHistory {
    id: number;
    sourceId: number;
    destinationId: number;
    amount: number;
    source: string;
    destination: string;
}

@Injectable({
    providedIn: 'root',
})
export class HistoryService {
    private readonly apiUrl = 'http://localhost:5000/api/v1/bookings/history';

    constructor(private http: HttpClient) { }

    getHistory(): Observable<BookingHistory[]> {
        return this.http.get<BookingHistory[]>(this.apiUrl);
    }
}

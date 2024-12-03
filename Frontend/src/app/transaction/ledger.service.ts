import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class LedgerService {
    private apiUrl = 'http://localhost:5000/api/v1/ledgers';

    constructor(private http: HttpClient) { }

    getLedgers(): Observable<any[]> {
        return this.http.get<any[]>(this.apiUrl);
    }
}

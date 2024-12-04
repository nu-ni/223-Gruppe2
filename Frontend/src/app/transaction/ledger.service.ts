import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

interface Ledger {
    id: number;
    name: string;
    balance: number;
}

@Injectable({
    providedIn: 'root',
})
export class LedgerService {
    private apiUrl = 'http://localhost:5000/api/v1/ledgers';

    constructor(private http: HttpClient) { }

    getLedgers(): Observable<Ledger[]> {
        return this.http.get<Ledger[]>(this.apiUrl);
    }

    getLedgerById(id: number): Promise<Ledger | null> {
        if (!id) {
            console.error('Invalid ledger ID passed:', id);
            return Promise.reject('Invalid ledger ID');
        }
    
        console.log('Fetching ledger with ID:', id);
    
        return this.http
            .get<Ledger>(`${this.apiUrl}/${id}`)
            .toPromise()
            .then((ledger) => {
                if (!ledger) {
                    console.warn('Ledger not found for ID:', id);
                    return null;
                }
                return ledger;
            })
            .catch((error) => {
                console.error('Error fetching ledger with ID:', id, error);
                return null; // Explicitly return null on error
            });
    }    
}

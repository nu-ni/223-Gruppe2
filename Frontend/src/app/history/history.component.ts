import { Component, OnInit } from '@angular/core';
import { BookingService } from '../transaction/booking.service';
import { LedgerService } from '../transaction/ledger.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

  interface Booking {
    id: number;
    sourceLedgerId: number;
    destinationLedgerId: number;
    amount: number;
    sourceLedgerName?: string;
    destinationLedgerName?: string;
  }
  
  interface Ledger {
    id: number;
    name: string;
    balance: number;
  }
  
  @Component({
    selector: 'app-history',
    imports: [CommonModule, FormsModule],
    templateUrl: './history.component.html',
    styleUrls: ['./history.component.css'],
  })

  export class HistoryComponent implements OnInit {
    bookings: Booking[] = [];
    ledgers: Ledger[] = [];
    isLoading: boolean = false;
    errorMessage: string | null = null;
  
    constructor(
      private bookingService: BookingService,
      private ledgerService: LedgerService
    ) {}
  
    ngOnInit(): void {
      this.fetchLedgers();
      this.fetchBookingHistory();
    }
  
    fetchLedgers(): void {
      this.isLoading = true;
      this.ledgerService.getLedgers().subscribe({
        next: (data: Ledger[]) => {
          this.ledgers = data;
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = 'Failed to load ledgers.';
          console.error(error);
          this.isLoading = false;
        },
      });
    }
  
    fetchBookingHistory(): void {
      this.isLoading = true;
      this.bookingService.getBookingHistory().subscribe({
        next: (data: Booking[]) => {
          this.bookings = data;
  
          // Collect all ledger IDs from bookings
          const ledgerIds = new Set<number>();
          data.forEach((booking) => {
            if (booking.sourceLedgerId) ledgerIds.add(booking.sourceLedgerId);
            if (booking.destinationLedgerId) ledgerIds.add(booking.destinationLedgerId);
          });
  
          // Fetch all ledgers by ID
          const ledgerRequests = Array.from(ledgerIds).map((id) =>
            this.ledgerService.getLedgerById(id)
          );
  
          Promise.all(ledgerRequests)
            .then((ledgers) => {
              const ledgerMap = new Map<number, string>();
              ledgers.forEach((ledger) => {
                if (ledger) ledgerMap.set(ledger.id, ledger.name);
              });
  
              // Map ledger names to bookings
              this.bookings = this.bookings.map((booking) => ({
                ...booking,
                sourceLedgerName:
                  ledgerMap.get(booking.sourceLedgerId) || 'Unknown',
                destinationLedgerName:
                  ledgerMap.get(booking.destinationLedgerId) || 'Unknown',
              }));
            })
            .catch((error) => {
              console.error('Failed to load ledger names', error);
              this.errorMessage = 'Failed to load ledger names.';
            })
            .finally(() => {
              this.isLoading = false;
            });
        },
        error: (error) => {
          this.errorMessage = 'Failed to load bookings.';
          console.error(error);
          this.isLoading = false;
        },
      });
    }
  
  }

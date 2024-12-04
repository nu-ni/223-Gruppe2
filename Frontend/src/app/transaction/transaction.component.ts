import { Component, OnInit, ViewChild } from '@angular/core';
import { LedgerService } from './ledger.service';
import { BookingService } from './booking.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HistoryComponent } from '../history/history.component';

@Component({
  selector: 'app-transaction',
  imports: [FormsModule, CommonModule],
  templateUrl: './transaction.component.html',
  styleUrls: ['./transaction.component.css'],
})

export class TransactionComponent implements OnInit {
  @ViewChild(HistoryComponent) historyComponent!: HistoryComponent; // Referenz zum HistoryComponent

  ledgers: any[] = [];
  debitLedger: string = '';
  creditLedger: string = '';
  debitLedgerBalance: number | null = null;
  creditLedgerBalance: number | null = null;
  amount: number | null = null;

  constructor(
    private ledgerService: LedgerService,
    private bookingService: BookingService
  ) {}

  ngOnInit(): void {
    this.fetchLedgers();
  }

  fetchLedgers() {
    this.ledgerService.getLedgers().subscribe(
      (data) => {
        this.ledgers = data;
      },
      (error) => {
        console.error('Failed to fetch ledgers', error);
      }
    );
  }

  updateDebitLedgerBalance(): void {
    const selectedLedger = this.ledgers.find(ledger => ledger.id === +this.debitLedger);
    this.debitLedgerBalance = selectedLedger ? selectedLedger.balance : null;
  }

  updateCreditLedgerBalance(): void {
    const selectedLedger = this.ledgers.find(ledger => ledger.id === +this.creditLedger);
    this.creditLedgerBalance = selectedLedger ? selectedLedger.balance : null;
  }

  swapLedgers(): void {
    const temp = this.debitLedger;
    this.debitLedger = this.creditLedger;
    this.creditLedger = temp;
    this.updateDebitLedgerBalance();
    this.updateCreditLedgerBalance();
  }

  resetForm(): void {
    this.debitLedger = '';
    this.creditLedger = '';
    this.amount = null;
    this.debitLedgerBalance = null;
    this.creditLedgerBalance = null;
  }

  onSubmit() {
    if (!this.debitLedger || !this.creditLedger || this.debitLedger === this.creditLedger || this.amount === null || this.amount <= 0) {
      alert('Invalid transaction details!');
      return;
    }

    this.bookingService.book(this.debitLedger, this.creditLedger, this.amount).subscribe({
      next: () => {
        alert('Transaction successfully booked!');
        this.resetForm(); // Formular zurücksetzen
      },
      error: (error) => {
        console.error('Booking failed:', error);
        alert('Failed to submit the transaction.');
      }
    });
  }
}

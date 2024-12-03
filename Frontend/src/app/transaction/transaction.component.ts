import { Component, OnInit } from '@angular/core';
import { LedgerService } from './ledger.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-transaction',
  imports: [FormsModule, CommonModule],
  templateUrl: './transaction.component.html',
  styleUrls: ['./transaction.component.css'],
})

export class TransactionComponent implements OnInit {
  ledgers: any[] = [];
  debitLedger: string = '';
  creditLedger: string = '';
  selectedDebitLedger: any; // The selected ledger object


  constructor(private ledgerService: LedgerService) {}

  ngOnInit(): void {
    this.fetchLedgers();
  }

  fetchLedgers() {
    this.ledgerService.getLedgers().subscribe(
      (data) => {
        this.ledgers = data; // Assign fetched ledgers to the dropdown
      },
      (error) => {
        console.error('Failed to fetch ledgers', error);
      }
    );
  }

  onSubmit() {
    if (this.debitLedger === this.creditLedger) {
      alert('Debit and Credit cannot be the same ledger!');
      return;
    }

    console.log('Transaction submitted', {
      debit: this.debitLedger,
      credit: this.creditLedger,
    });
  }
  updateSelectedLedger(ledgerId: number): void {
    // Find the ledger by ID and update the selectedDebitLedger property
    this.selectedDebitLedger = this.ledgers.find(ledger => ledger.id === ledgerId);
  }
}

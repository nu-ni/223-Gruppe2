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
  debitLedgerBalance: number | null = null;
  creditLedgerBalance: number | null = null;
  amount: number | null = null; // Transaction amount

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

  onSubmit() {
    // Check if Debit Ledger is selected
    if (!this.debitLedger) {
      alert('Please select a Debit Ledger.');
      return;
    }
  
    // Check if Credit Ledger is selected
    if (!this.creditLedger) {
      alert('Please select a Credit Ledger.');
      return;
    }
  
    // Check if Debit and Credit Ledgers are the same
    if (this.debitLedger === this.creditLedger) {
      alert('Debit and Credit cannot be the same ledger!');
      return;
    }
  
    // Check if the transaction amount is provided and valid
    if (this.amount === null || this.amount <= 0) {
      alert('The amount must be a positive number!');
      return;
    }
  
    // Check if the transaction amount exceeds the balance of the debit ledger
    if (this.debitLedgerBalance !== null && this.amount > this.debitLedgerBalance) {
      alert('Insufficient funds in the debit ledger!');
      return;
    }
  
    // Check if the transaction amount exceeds 100 million
    if (this.amount > 100_000_000) {
      alert('The transaction amount cannot exceed 100 million!');
      return;
    }
  
    console.log('Transaction submitted', {
      debit: this.debitLedger,
      credit: this.creditLedger,
      amount: this.amount,
    });
  
    alert('Transaction successfully submitted!');
  }
}

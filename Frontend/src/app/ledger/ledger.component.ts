import { Component, OnInit } from '@angular/core';
import { LedgerService } from '../../services/ledger.service';
import { Ledger } from '../../models/ledger.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { TransactionComponent } from '../transaction/transaction.component';
import { HistoryComponent } from '../history/history.component';

@Component({
  selector: 'app-ledger',
  templateUrl: './ledger.component.html',
  styleUrls: ['./ledger.component.css'],
  imports: [CommonModule, FormsModule, TransactionComponent, HistoryComponent],
  standalone: true,
  providers:  [ LedgerService, HttpClient ]
})
export class LedgerComponent implements OnInit {
  ledgers: Ledger[] = [];
  fromLedgerId: number | null = null;
  toLedgerId: number | null = null;
  amount: number | null = null;
  transferMessage: string = '';

  constructor(private ledgerService: LedgerService) {}

  ngOnInit(): void {
    this.loadLedgers();
  }

  loadLedgers(): void {
    this.ledgerService.getLedgers().subscribe(
      (data) => (this.ledgers = data),
      (error) => console.error('Error fetching ledgers', error)
    );
  }

  makeTransfer(): void {
    if (
      this.fromLedgerId !== null &&
      this.toLedgerId !== null &&
      this.amount !== null &&
      this.amount > 0
    ) {
      this.ledgerService
        .transferFunds(this.fromLedgerId, this.toLedgerId, this.amount)
        .subscribe(
          () => {
            this.transferMessage = 'Transfer successful!';
            this.loadLedgers(); // Refresh ledger balances
          },
          (error) => {
            this.transferMessage = `Transfer failed: ${error.error.message}`;
            console.error('Transfer error', error);
          }
        );
    } else {
      this.transferMessage = 'Please fill in all fields with valid data.';
    }
  }

  deleteLedger(id: number) {
    this.ledgerService.deleteLedger(id).subscribe(
      () => {
        this.loadLedgers();
      },
      (error) => {
        console.error('Error deleting ledger', error);
      }
    );
    }
}

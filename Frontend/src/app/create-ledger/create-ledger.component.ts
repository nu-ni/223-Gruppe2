import {Component, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {LedgerService} from '../../services/ledger.service';
import {HttpClient} from '@angular/common/http';
import {AuthService} from '../../services/auth.service';
import {Router} from '@angular/router';

@Component({
  selector: 'app-create-ledger',
  templateUrl: './create-ledger.component.html',
  imports: [CommonModule, FormsModule],
  standalone: true,
  providers:  [ LedgerService, HttpClient ]
})
export class CreateLedgerComponent {
  ledgerName: string = '';
  message: string = '';
  constructor(private ledgerService: LedgerService, private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    if (this.ledgerName.trim()) {
      this.ledgerService.createLedger(this.ledgerName).subscribe({
        next: (response: any) => {
          console.log('Ledger created', response);
          // Handle success (e.g., navigate to another page)
          this.router.navigate(['/ledgers']);
        },
        error: (err) => {
          console.error('Ledger creation failed', err);
          this.message = 'Ledger creation failed';
        },
      });
      console.log('Creating ledger', this.ledgerName);
      /*this.router.navigate(['/ledgers']);*/
    } else {
      this.message = 'Please enter a valid ledger name';
  }}
}

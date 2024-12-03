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
  styleUrls: ['./create-ledger.component.css'],
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
      console.log('Creating ledger', this.ledgerName);
      /*this.router.navigate(['/ledgers']);*/
    } else {
      this.message = 'Please enter a valid ledger name';
  }}
}

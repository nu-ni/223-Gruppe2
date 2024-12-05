import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HistoryService, BookingHistory } from './history.service';

@Component({
  selector: 'app-history',
  standalone: true,
  templateUrl: './history.component.html',
  imports: [FormsModule, CommonModule],
  styleUrls: ['./history.component.css'],
})
export class HistoryComponent implements OnInit {
  history: BookingHistory[] = [];
  loading: boolean = true;
  error: string | null = null;

  constructor(private historyService: HistoryService) {}

  ngOnInit(): void {
    this.fetchHistory();
  }

  fetchHistory(): void {
    this.loading = true;
    this.error = null;
    this.historyService.getHistory().subscribe({
      next: (data) => {
        this.history = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching history:', err);
        this.error = 'Failed to load booking history.';
        this.loading = false;
      },
    });
  }
}

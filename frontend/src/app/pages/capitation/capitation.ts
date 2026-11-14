import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiClient, CapitationDetail, CapitationSummary } from '../../core/api-client';

@Component({
  imports: [FormsModule, CurrencyPipe, DatePipe, DecimalPipe],
  selector: 'app-capitation',
  styleUrl: './capitation.scss',
  templateUrl: './capitation.html',
})
export class Capitation {
  private readonly api = inject(ApiClient);

  readonly months = signal<string[]>([]);
  readonly month = signal('');
  readonly summary = signal<CapitationSummary | null>(null);
  readonly detail = signal<CapitationDetail | null>(null);
  readonly selectedContractId = signal<number | null>(null);
  readonly loading = signal(false);
  readonly message = signal<string | null>(null);

  readonly pdfUrl = computed(() => {
    const contractId = this.selectedContractId();
    return contractId ? this.api.getCapitationPdfUrl(contractId, this.month()) : null;
  });

  constructor() {
    this.api.getCapitationMonths().subscribe({
      next: (months) => {
        this.months.set(months);
        if (months.length === 0) {
          this.message.set('No MMR data has been loaded yet.');
          return;
        }
        this.month.set(months[0]);
        this.loadSummary();
      },
      error: () => this.message.set('Could not load the MMR months.'),
    });
  }

  onMonthChange(month: string): void {
    this.month.set(month);
    this.loadSummary();
  }

  selectContract(contractId: number): void {
    this.selectedContractId.set(contractId);
    this.detail.set(null);
    this.api.getCapitationDetail(contractId, this.month()).subscribe({
      next: (detail) => this.detail.set(detail),
      error: (err: HttpErrorResponse) => this.fail(err),
    });
  }

  private loadSummary(): void {
    this.loading.set(true);
    this.message.set(null);
    this.summary.set(null);
    this.detail.set(null);
    this.selectedContractId.set(null);

    this.api.getCapitationSummary(this.month()).subscribe({
      next: (summary) => {
        this.summary.set(summary);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => this.fail(err),
    });
  }

  private fail(err: HttpErrorResponse): void {
    this.loading.set(false);
    this.message.set(
      err.status === 404
        ? (err.error?.detail ?? 'No MMR data found for that month.')
        : 'Could not load capitation.',
    );
  }
}

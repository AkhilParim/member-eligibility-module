import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiClient, IdCardData } from '../../core/api-client';

@Component({
  imports: [RouterLink],
  selector: 'app-id-card',
  styleUrl: './id-card.scss',
  templateUrl: './id-card.html',
})
export class IdCard {
  private readonly api = inject(ApiClient);
  private readonly route = inject(ActivatedRoute);

  readonly memberId = Number(this.route.snapshot.paramMap.get('memberId'));
  readonly card = signal<IdCardData | null>(null);
  readonly loading = signal(true);

  constructor() {
    this.api.getIdCard(this.memberId).subscribe({
      next: (card) => {
        this.card.set(card);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  get pdfUrl(): string {
    return this.api.getIdCardPdfUrl(this.memberId);
  }
}

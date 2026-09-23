import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiClient, ClaimSummary, Eob } from '../../core/api-client';

@Component({
  imports: [RouterLink],
  selector: 'app-claims-eob',
  styleUrl: './claims-eob.scss',
  templateUrl: './claims-eob.html',
})
export class ClaimsEob {
  private readonly api = inject(ApiClient);
  private readonly route = inject(ActivatedRoute);

  readonly memberId = Number(this.route.snapshot.paramMap.get('memberId'));
  readonly claims = signal<ClaimSummary[]>([]);
  readonly loadingClaims = signal(true);

  readonly selectedEob = signal<Eob | null>(null);
  readonly loadingEob = signal(false);

  constructor() {
    this.api.getClaims(this.memberId).subscribe({
      next: (claims) => {
        this.claims.set(claims);
        this.loadingClaims.set(false);
      },
      error: () => this.loadingClaims.set(false),
    });
  }

  viewEob(claimId: number): void {
    this.loadingEob.set(true);
    this.selectedEob.set(null);
    this.api.getEob(claimId).subscribe({
      next: (eob) => {
        this.selectedEob.set(eob);
        this.loadingEob.set(false);
      },
      error: () => this.loadingEob.set(false),
    });
  }

  get eobPdfUrl(): string | null {
    const eob = this.selectedEob();
    return eob ? this.api.getEobPdfUrl(eob.claimId) : null;
  }
}

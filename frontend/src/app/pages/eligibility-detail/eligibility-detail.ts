import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiClient, Eligibility } from '../../core/api-client';

@Component({
  imports: [RouterLink],
  selector: 'app-eligibility-detail',
  styleUrl: './eligibility-detail.scss',
  templateUrl: './eligibility-detail.html',
})
export class EligibilityDetail {
  private readonly api = inject(ApiClient);
  private readonly route = inject(ActivatedRoute);

  readonly memberId = Number(this.route.snapshot.paramMap.get('memberId'));
  readonly records = signal<Eligibility[]>([]);
  readonly loading = signal(true);

  constructor() {
    this.api.getEligibility(this.memberId).subscribe({
      next: (records) => {
        this.records.set(records);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}

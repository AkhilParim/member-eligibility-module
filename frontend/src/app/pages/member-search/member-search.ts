import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiClient, MemberSearchResult } from '../../core/api-client';

@Component({
  imports: [FormsModule],
  selector: 'app-member-search',
  styleUrl: './member-search.scss',
  templateUrl: './member-search.html',
})
export class MemberSearch {
  private readonly api = inject(ApiClient);
  private readonly router = inject(Router);

  readonly searchTerm = signal('');
  readonly results = signal<MemberSearchResult[]>([]);
  readonly loading = signal(false);
  readonly searched = signal(false);

  constructor() {
    // Prototype with only a few members — list everyone by default until searched.
    this.search();
  }

  search(): void {
    this.loading.set(true);
    this.searched.set(true);
    this.api.searchMembers(this.searchTerm()).subscribe({
      next: (members) => {
        this.results.set(members);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openMember(memberId: number): void {
    this.router.navigate(['/members', memberId, 'eligibility']);
  }
}

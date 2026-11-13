import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiClient, MemberEob, MemberEop } from '../../core/api-client';

type Mode = 'eob' | 'eop';

@Component({
  imports: [FormsModule, RouterLink, DecimalPipe],
  selector: 'app-statements',
  styleUrl: './statements.scss',
  templateUrl: './statements.html',
})
export class Statements {
  private readonly api = inject(ApiClient);
  private readonly route = inject(ActivatedRoute);

  readonly memberId = Number(this.route.snapshot.paramMap.get('memberId'));

  readonly mode = signal<Mode>('eob');
  readonly from = signal('');
  readonly to = signal('');
  readonly applied = signal<{ from?: string; to?: string }>({});

  readonly eob = signal<MemberEob | null>(null);
  readonly eop = signal<MemberEop | null>(null);
  readonly loading = signal(false);
  readonly message = signal<string | null>(null);

  readonly pdfUrl = computed(() => {
    const { from, to } = this.applied();
    return this.mode() === 'eob'
      ? this.api.getMemberEobPdfUrl(this.memberId, from, to)
      : this.api.getMemberEopPdfUrl(this.memberId, from, to);
  });

  constructor() {
    this.load();
  }

  setMode(mode: Mode): void {
    if (mode === this.mode()) return;
    this.mode.set(mode);
    this.load();
  }

  clear(): void {
    this.from.set('');
    this.to.set('');
    this.load();
  }

  load(): void {
    const from = this.from() || undefined;
    const to = this.to() || undefined;

    if (from && to && from > to) {
      this.message.set("The 'From' date must be on or before the 'To' date.");
      return;
    }

    this.applied.set({ from, to });
    this.loading.set(true);
    this.message.set(null);

    if (this.mode() === 'eob') {
      this.eob.set(null);
      this.api.getMemberEob(this.memberId, from, to).subscribe({
        next: (doc) => {
          this.eob.set(doc);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => this.fail(err),
      });
    } else {
      this.eop.set(null);
      this.api.getMemberEop(this.memberId, from, to).subscribe({
        next: (doc) => {
          this.eop.set(doc);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => this.fail(err),
      });
    }
  }

  private fail(err: HttpErrorResponse): void {
    this.loading.set(false);
    this.message.set(
      err.status === 404
        ? (err.error?.detail ?? 'No claims found for this member.')
        : 'Could not load the statement.',
    );
  }
}

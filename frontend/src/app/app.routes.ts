import { Routes } from '@angular/router';
import { MemberSearch } from './pages/member-search/member-search';
import { EligibilityDetail } from './pages/eligibility-detail/eligibility-detail';
import { IdCard } from './pages/id-card/id-card';
import { ClaimsEob } from './pages/claims-eob/claims-eob';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'search' },
  { path: 'search', component: MemberSearch },
  { path: 'members/:memberId/eligibility', component: EligibilityDetail },
  { path: 'members/:memberId/id-card', component: IdCard },
  { path: 'members/:memberId/claims', component: ClaimsEob },
];

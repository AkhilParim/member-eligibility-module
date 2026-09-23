import { HttpClient } from '@angular/common/http';
import { Service, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface MemberSearchResult {
  memberId: number;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  planName: string | null;
  eligibilityStatus: 'eligible' | 'not_eligible';
}

export interface Eligibility {
  memberId: number;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  enrollmentId: number;
  subscriberNumber: string;
  relationship: string;
  effectiveDate: string;
  termDate: string | null;
  eligibilityStatus: 'eligible' | 'not_eligible';
  planId: number;
  planName: string;
  planType: string;
  groupNumber: string;
  pcpName: string | null;
  pcpPhone: string | null;
  copayPcp: number;
  copaySpecialist: number;
  copayEr: number;
}

export interface IdCardData {
  cardId: number;
  memberId: number;
  memberName: string;
  dateOfBirth: string;
  subscriberNumber: string;
  planName: string;
  planType: string;
  groupNumber: string;
  payerId: string;
  cmsContractNumber: string | null;
  planBenefitPackageNumber: string | null;
  customerServicePhone: string | null;
  memberServicesPhone: string | null;
  behavioralHealthPhone: string | null;
  pharmacyHelpDeskPhone: string | null;
  website: string | null;
  claimsAddress: string | null;
  claimInquiryPhone: string | null;
  copayPcp: number;
  copaySpecialist: number;
  copayEr: number;
  effectiveDate: string;
  rxBin: string | null;
  rxPcn: string | null;
  rxGrp: string | null;
  rxId: string | null;
  issuedDate: string;
}

export interface ClaimSummary {
  claimId: number;
  claimNumber: string;
  providerName: string;
  dateReceived: string;
  datePaid: string | null;
  claimStatus: string;
  totalBilled: number;
  totalWhatYouOwe: number;
}

export interface EobLine {
  lineNo: number;
  dateOfService: string;
  serviceDescription: string;
  claimStatus: string;
  providerCharges: number;
  allowedCharges: number;
  copay: number;
  deductible: number;
  coinsurance: number;
  paidByInsurer: number;
  whatYouOwe: number;
  remarkCode: string | null;
}

export interface EobTotals {
  providerCharges: number;
  allowedCharges: number;
  copay: number;
  deductible: number;
  coinsurance: number;
  paidByInsurer: number;
  whatYouOwe: number;
}

export interface Eob {
  claimId: number;
  claimNumber: string;
  documentNumber: string;
  statementDate: string;
  dateReceived: string;
  datePaid: string | null;
  claimStatus: string;
  providerName: string;
  payeeName: string;
  memberId: number;
  memberName: string;
  address: string | null;
  city: string | null;
  state: string | null;
  zip: string | null;
  subscriberNumber: string;
  groupNumber: string;
  customerServicePhone: string | null;
  lines: EobLine[];
  totals: EobTotals;
}

@Service()
export class ApiClient {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  searchMembers(search: string): Observable<MemberSearchResult[]> {
    return this.http.get<MemberSearchResult[]>(`${this.baseUrl}/members`, {
      params: search ? { search } : {},
    });
  }

  getEligibility(memberId: number): Observable<Eligibility[]> {
    return this.http.get<Eligibility[]>(`${this.baseUrl}/members/${memberId}/eligibility`);
  }

  getIdCard(memberId: number): Observable<IdCardData> {
    return this.http.get<IdCardData>(`${this.baseUrl}/members/${memberId}/idcard`);
  }

  getIdCardPdfUrl(memberId: number): string {
    return `${this.baseUrl}/members/${memberId}/idcard/pdf`;
  }

  getClaims(memberId: number): Observable<ClaimSummary[]> {
    return this.http.get<ClaimSummary[]>(`${this.baseUrl}/members/${memberId}/claims`);
  }

  getEob(claimId: number): Observable<Eob> {
    return this.http.get<Eob>(`${this.baseUrl}/claims/${claimId}/eob`);
  }

  getEobPdfUrl(claimId: number): string {
    return `${this.baseUrl}/claims/${claimId}/eob/pdf`;
  }
}

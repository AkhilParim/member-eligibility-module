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

export interface MemberEobClaim {
  claimId: number;
  claimNumber: string;
  documentNumber: string;
  statementDate: string;
  dateReceived: string;
  datePaid: string | null;
  claimStatus: string;
  providerName: string;
  payeeName: string;
  lines: EobLine[];
  totals: EobTotals;
}

export interface MemberEob {
  memberId: number;
  memberName: string;
  address: string | null;
  city: string | null;
  state: string | null;
  zip: string | null;
  subscriberNumber: string | null;
  groupNumber: string | null;
  planName: string | null;
  customerServicePhone: string | null;
  statementDate: string;
  periodFrom: string | null;
  periodTo: string | null;
  claims: MemberEobClaim[];
  totals: EobTotals;
}

export interface EopTotals {
  billed: number;
  contractualAdjustment: number;
  allowed: number;
  copay: number;
  deductible: number;
  coinsurance: number;
  patientResponsibility: number;
  paid: number;
}

export interface EopLine {
  lineNo: number;
  dateOfService: string;
  procedureCode: string | null;
  serviceDescription: string;
  lineStatus: string;
  billed: number;
  contractualAdjustment: number;
  allowed: number;
  copay: number;
  deductible: number;
  coinsurance: number;
  patientResponsibility: number;
  paid: number;
  reasonCode: string | null;
  remarkCode: string | null;
}

export interface EopClaim {
  claimId: number;
  claimNumber: string;
  documentNumber: string;
  providerName: string;
  claimStatus: string;
  dateReceived: string;
  lines: EopLine[];
  totals: EopTotals;
}

export interface EopPayment {
  payeeName: string;
  providerNpi: string | null;
  paymentNumber: string | null;
  paymentMethod: string | null;
  paymentDate: string | null;
  claimCount: number;
  claims: EopClaim[];
  totals: EopTotals;
}

export interface MemberEop {
  memberId: number;
  memberName: string;
  subscriberNumber: string | null;
  groupNumber: string | null;
  payerName: string | null;
  payerId: string | null;
  payerPhone: string | null;
  payerAddress: string | null;
  statementDate: string;
  periodFrom: string | null;
  periodTo: string | null;
  claimCount: number;
  payments: EopPayment[];
  totals: EopTotals;
}

export interface CapitationTotals {
  memberCount: number;
  memberMonths: number;
  mmrPremium: number;
  capitation: number;
  capitationPercentOfPremium: number | null;
}

export interface CapitationSummaryContract {
  contractId: number;
  contractNumber: string;
  providerGroupName: string;
  pmpmRate: number;
  memberCount: number;
  memberMonths: number;
  mmrPremium: number;
  capitation: number;
  capitationPercentOfPremium: number | null;
}

export interface CapitationSummary {
  paymentMonth: string;
  contracts: CapitationSummaryContract[];
  totals: CapitationTotals;
}

export interface CapitationContract {
  contractId: number;
  contractNumber: string;
  providerGroupName: string;
  providerTin: string | null;
  pmpmRate: number;
  effectiveDate: string;
  termDate: string | null;
}

export interface CapitationMember {
  memberId: number;
  memberName: string;
  mbi: string | null;
  riskFactor: number | null;
  regularMonths: number;
  adjustmentMonths: number;
  memberMonths: number;
  mmrPremium: number;
  capitation: number;
}

export interface CapitationDetail {
  paymentMonth: string;
  contract: CapitationContract;
  members: CapitationMember[];
  totals: CapitationTotals;
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

  getMemberEob(memberId: number, from?: string, to?: string): Observable<MemberEob> {
    return this.http.get<MemberEob>(`${this.baseUrl}/members/${memberId}/eob`, {
      params: this.rangeParams(from, to),
    });
  }

  getMemberEop(memberId: number, from?: string, to?: string): Observable<MemberEop> {
    return this.http.get<MemberEop>(`${this.baseUrl}/members/${memberId}/eop`, {
      params: this.rangeParams(from, to),
    });
  }

  getMemberEobPdfUrl(memberId: number, from?: string, to?: string): string {
    return `${this.baseUrl}/members/${memberId}/eob/pdf${this.rangeQuery(from, to)}`;
  }

  getMemberEopPdfUrl(memberId: number, from?: string, to?: string): string {
    return `${this.baseUrl}/members/${memberId}/eop/pdf${this.rangeQuery(from, to)}`;
  }

  getCapitationMonths(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/capitation/months`);
  }

  getCapitationSummary(month: string): Observable<CapitationSummary> {
    return this.http.get<CapitationSummary>(`${this.baseUrl}/capitation`, { params: { month } });
  }

  getCapitationDetail(contractId: number, month: string): Observable<CapitationDetail> {
    return this.http.get<CapitationDetail>(`${this.baseUrl}/capitation/${contractId}`, { params: { month } });
  }

  getCapitationPdfUrl(contractId: number, month: string): string {
    return `${this.baseUrl}/capitation/${contractId}/pdf?month=${encodeURIComponent(month)}`;
  }

  private rangeParams(from?: string, to?: string): Record<string, string> {
    const params: Record<string, string> = {};
    if (from) params['from'] = from;
    if (to) params['to'] = to;
    return params;
  }

  private rangeQuery(from?: string, to?: string): string {
    const query = new URLSearchParams(this.rangeParams(from, to)).toString();
    return query ? `?${query}` : '';
  }
}

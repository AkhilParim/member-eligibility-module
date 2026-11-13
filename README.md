# Member Eligibility Module

A healthcare **member eligibility** take-home project: Angular + ASP.NET Core Web
API + PostgreSQL, with generated **member ID card** and **Explanation of Benefits
(EOB)** artifacts modeled directly on real, downloaded CMS samples.

## How this maps to the assignment

| Requirement | What was built |
|---|---|
| Learn .NET, build an app | ASP.NET Core 8 (net10.0) Web API — `backend/` |
| Learn SQL, build a DB | PostgreSQL schema, stored procedures, seed data — authored as hand-written SQL, applied via EF Core migrations (see below) (the separate "payroll employee module" exercise was explicitly descoped for this project) |
| Member eligibility module (healthcare) | Full member/plan/enrollment/claims data model + eligibility lookup API and screen |
| Up to 4 screens | 4 core Angular routes (Member Search, Eligibility Detail, ID Card, Claims & EOB), plus a Statements screen added in a follow-up expansion |
| Up to 6 DB tables | `members`, `plans`, `enrollments`, `id_cards`, `claims`, `claim_lines` |
| Produce an ID card online, use as data points | Fields modeled on the **CMS official sample dual-eligible Member ID Card template**; rendered on-screen and as a downloadable PDF |
| Produce an EOB, use as data points | Fields and one seeded claim's dollar amounts taken directly from the **CMS "Reading Your Explanation of Benefits" sample (Pub #11819, go.cms.gov/c2c)**; rendered on-screen and as a downloadable PDF |

## Stack

- **Frontend:** Angular 22 (standalone components, signals) + TypeScript
- **Backend:** ASP.NET Core 8 Web API (C#), EF Core (Npgsql), raw-SQL calls into
  PL/pgSQL stored functions, QuestPDF for PDF rendering
- **Database:** PostgreSQL 16 (via Docker)

## Project layout

```
backend/                ASP.NET Core Web API
backend/Migrations/     EF Core migrations — the versioned source of truth for schema,
                        stored procedures, and seed data (see "Schema & migrations" below)
frontend/               Angular app
docker-compose.yml      Postgres only — starts empty; migrations own the schema
```

## Data model (6 tables)

- **members** — demographic info
- **plans** — plan/payer details, phone numbers, copays (from the ID card sample)
- **enrollments** — the eligibility record: member ↔ plan, coverage window, PCP
- **id_cards** — RxBIN/RxPCN/RxGRP/RxID + issue history per member
- **claims** — claim header (provider, payee, dates, statement/document numbers, plus payment
  number/method and provider NPI used by the Explanation of Payment)
- **claim_lines** — per-line EOB detail (billed/allowed/copay/deductible/
  coinsurance/paid/owed/remark code, plus procedure code and adjustment reason code)

## Stored procedures

- `fn_get_member_eligibility(member_id, as_of)` — eligibility status as of a date
- `fn_get_id_card_data(member_id)` — joined data for rendering the ID card
- `fn_get_eob(claim_id)` — claim + line items + totals as JSON (mirrors the CMS
  sample's Total row)
- `sp_enroll_member(...)` — opens a new enrollment, term-dating any prior
  overlapping one (used by `POST /api/enrollments`)
- `fn_get_member_eob(member_id, from, to)` — every claim for a member with lines and
  totals, as JSON (member-level Explanation of Benefits)
- `fn_get_member_eop(member_id, from, to)` — the member's processed claims grouped by
  payment (payee + payment number), as JSON (provider-facing Explanation of Payment)

These are called from the API via parameterized raw SQL (`Services/EligibilityFunctions.cs`),
not reimplemented in LINQ. They're created by `backend/Migrations/20260915221912_StoredProcedures.cs`.

## Schema & migrations

Schema, stored procedures, and seed data are all **hand-written SQL**, applied via
**EF Core migrations** rather than `docker-entrypoint-initdb.d` scripts or
EF's auto-generated (model-diffed) DDL:

- `backend/Migrations/20260915221904_InitialSchema.cs` — the 6 tables, constraints, indexes
- `backend/Migrations/20260915221912_StoredProcedures.cs` — the first 4 functions above
- `backend/Migrations/20260915221914_SeedData.cs` — demo data (local dev only — see note below)
- `backend/Migrations/20260923013723_FixSeedPlanNames.cs` — corrects plan names in already-seeded databases
- `backend/Migrations/20261013230443_AddPaymentDetails.cs` — payment/NPI/procedure/reason-code columns (no new table, so the schema stays at 6 tables)
- `backend/Migrations/20261013230446_AddMemberStatementFunctions.cs` — `fn_get_member_eob` and `fn_get_member_eop`
- `backend/Migrations/20261013230450_SeedAdditionalClaims.cs` — more demo claims per member, incl. one denial (local dev only)

Each migration's `Up()`/`Down()` wraps plain SQL via `migrationBuilder.Sql(...)`
instead of letting EF generate DDL from the C# model — that auto-generated DDL
loses things like `VARCHAR` lengths, the `CHECK` constraint on `enrollments`, and
the `RESTRICT` vs `CASCADE` distinction on foreign keys, and doesn't know about
PL/pgSQL functions at all. `backend/Data/AppDbContext.cs` still maps the C# model
onto these tables for querying — it just doesn't own the DDL.

EF tracks which migrations have run in a `__EFMigrationsHistory` table it creates
in the database, so `dotnet ef database update` is idempotent and safe to run
against an environment repeatedly — it only applies what's missing.

> **Seed data caveat:** the `SeedData` migration inserts fabricated demo members —
> fine for local dev, but a real production pipeline would exclude it (e.g. keep
> it out of the shared migration history entirely, or gate it behind an
> environment check) rather than run it against a live database.

## Running it locally

1. **Database**
   ```bash
   docker compose up -d
   ```
   Starts Postgres on `localhost:55432`, empty — no schema yet.

2. **Apply migrations**
   ```bash
   cd backend
   dotnet tool restore      # first time only — installs the pinned dotnet-ef version
   dotnet ef database update
   ```

3. **Backend API**
   ```bash
   dotnet run --launch-profile http
   ```
   Listens on `http://localhost:5152`. Connection string is in
   `appsettings.json`.

4. **Frontend**
   ```bash
   cd frontend
   npm install   # first time only
   ng serve
   ```
   Opens on `http://localhost:4200` and talks to the API (CORS is pre-configured
   for this origin).

   > Note: this app requires a newer Node than some systems ship with by default
   > for the latest Angular CLI. If needed: `nvm install --lts && nvm use --lts`,
   > then `npm install -g @angular/cli@latest`.

## Seed data

Three fabricated demo members (no real PII) across two plans — one $0-copay
dual-eligible SNP plan and one PPO plan with copays/deductible/coinsurance — with
claims/EOBs covering both cost-share styles. Member 1 (Maria Gonzalez)'s claim
`CLM00098765` intentionally reuses the exact dollar figures from the CMS sample
EOB, so the rendered output can be checked line-for-line against that reference.

Each member has several claims across multiple providers (member 1 also has a denied
claim), so the member-level EOB and EOP have something to aggregate.

## Member statements: EOB and EOP

Both documents are built from a member's claims, by the stored functions above, and
are available as JSON, as a PDF, and on the **Statements** screen.

- **Explanation of Benefits (EOB)** is member-facing: every claim with its lines and a
  statement total. Same table as the single-claim EOB, based on the CMS sample EOB.
- **Explanation of Payment (EOP)** is provider-facing remittance: the processed claims
  grouped by payment (payee + payment number), showing billed, contractual adjustment,
  allowed, patient responsibility and provider paid. Columns follow the CMS Standard
  Paper Remittance Advice. Denied claims appear with a $0 payment; pending claims are
  left out.
- **Date range:** `from` / `to` are optional and filter on each line's date of service.
  A claim with no in-range lines is omitted, and totals cover only the in-range lines.
  A member with no claims in the period gets a 404; `from` after `to` gets a 400.
- Amounts reconcile at every level: billed - adjustment = allowed, and
  allowed - patient responsibility = provider paid.

## API surface

| Screen | Endpoint |
|---|---|
| Member Search | `GET /api/members?search=` |
| Eligibility Detail | `GET /api/members/{id}/eligibility?asOf=` |
| ID Card | `GET /api/members/{id}/idcard`, `GET /api/members/{id}/idcard/pdf` |
| Claims & EOB | `GET /api/members/{id}/claims`, `GET /api/claims/{claimId}/eob`, `GET /api/claims/{claimId}/eob/pdf` |
| Statements (member EOB) | `GET /api/members/{id}/eob?from=&to=`, `GET /api/members/{id}/eob/pdf?from=&to=` |
| Statements (provider EOP) | `GET /api/members/{id}/eop?from=&to=`, `GET /api/members/{id}/eop/pdf?from=&to=` |
| (bonus) Enroll | `POST /api/enrollments` — demonstrates `sp_enroll_member` |

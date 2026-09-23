# Deploying to Azure

Infrastructure is Terraform (`deploy/terraform/`); app deployment is GitHub
Actions (`.github/workflows/deploy.yml`), triggered on push to `main`.

## 0. Prerequisites

- An Azure account with billing enabled (**you create this — nothing here can**).
- `az`, `terraform`, and `gh` installed and on `PATH`.
- `gh auth status` shows you logged into the GitHub account that should own the
  deployed app's repo.

## 1. Log in to Azure

```bash
az login
```

Opens a browser to authenticate. Confirm the right subscription is selected:

```bash
az account show --output table
```

## 2. Review the plan before creating anything billed

```bash
cd deploy/terraform
terraform init
terraform plan
```

Read the resource list. Expected recurring cost (see the table in the root
`README.md` / the plan this was built from): **~$25–30/month** — App Service B1
(~$13/mo) + Postgres Flexible Server B1ms (~$12–15/mo); Static Web Apps and Key
Vault are effectively free at this scale. Nothing is created until `apply`.

## 3. Provision

```bash
terraform apply
```

Takes several minutes (Postgres Flexible Server is the slow part). On success:

```bash
terraform output                          # non-sensitive outputs
terraform output -raw migrator_connection_string
terraform output -raw app_connection_string
terraform output -raw static_web_app_deployment_token
```

## 4. Verify the EF Core migrations run cleanly

Sanity-check before wiring up CI — apply migrations once by hand using the
migrator connection string from step 3:

```bash
cd ../../backend
ConnectionStrings__Default="$(terraform -chdir=../deploy/terraform output -raw migrator_connection_string)" \
  dotnet ef database update
```

## 5. Set GitHub Actions secrets

From the repo root:

```bash
API_URL=$(terraform -chdir=deploy/terraform output -raw api_url)

gh secret set AZURE_RESOURCE_GROUP           --body "$(terraform -chdir=deploy/terraform output -raw resource_group_name)"
gh secret set AZURE_WEBAPP_NAME              --body "$(terraform -chdir=deploy/terraform output -raw app_service_name)"
gh secret set AZURE_POSTGRES_SERVER_NAME     --body "$(terraform -chdir=deploy/terraform output -raw postgres_fqdn | cut -d. -f1)"
gh secret set MIGRATOR_DB_CONNECTION_STRING  --body "$(terraform -chdir=deploy/terraform output -raw migrator_connection_string)"
gh secret set API_BASE_URL                   --body "$API_URL"
gh secret set SWA_DEPLOYMENT_TOKEN           --body "$(terraform -chdir=deploy/terraform output -raw static_web_app_deployment_token)"
```

`AZURE_CREDENTIALS` needs a service principal (Terraform doesn't create this —
it's a CI identity, not app infrastructure):

```bash
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
RG=$(terraform -chdir=deploy/terraform output -raw resource_group_name)

az ad sp create-for-rbac \
  --name "member-eligibility-deploy" \
  --role contributor \
  --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG" \
  --output json > /tmp/sp.json

jq -n \
  --arg clientId "$(jq -r .appId /tmp/sp.json)" \
  --arg clientSecret "$(jq -r .password /tmp/sp.json)" \
  --arg tenantId "$(jq -r .tenant /tmp/sp.json)" \
  --arg subscriptionId "$SUBSCRIPTION_ID" \
  '{clientId:$clientId, clientSecret:$clientSecret, tenantId:$tenantId, subscriptionId:$subscriptionId}' \
  | gh secret set AZURE_CREDENTIALS

rm /tmp/sp.json
```

## 6. Deploy

```bash
git push origin main
```

Watch the run: `gh run watch`. On success, `terraform output frontend_url` is
the live demo link.

## Notes / known simplifications

- Terraform state is local (`deploy/terraform/terraform.tfstate`, gitignored) —
  fine for a single-operator demo project; a team setup would use a remote
  backend (Azure Storage) instead.
- `AZURE_CREDENTIALS` is a long-lived service principal secret. The more
  current-best-practice approach is OIDC federated credentials (no stored
  secret at all) — skipped here to keep the Azure AD app-registration setup
  out of scope for a demo deploy; worth revisiting if this becomes a real,
  ongoing service.
- The migration job opens/closes a Postgres firewall rule for the GitHub
  Actions runner's IP on every run, since hosted runners don't have a fixed
  IP range worth allow-listing permanently.

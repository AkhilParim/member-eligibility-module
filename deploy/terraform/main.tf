data "azurerm_client_config" "current" {}

# Used to open the Postgres firewall just wide enough for the machine running
# `terraform apply` (the postgresql provider connects directly during apply).
data "http" "operator_ip" {
  url = "https://api.ipify.org"
}

resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

resource "random_password" "postgres_admin" {
  length  = 24
  special = false
}

resource "random_password" "migrator_role" {
  length  = 24
  special = false
}

resource "random_password" "app_role" {
  length  = 24
  special = false
}

resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

# --- Database ---------------------------------------------------------

resource "azurerm_postgresql_flexible_server" "main" {
  name                          = "${var.name_prefix}-pg-${random_string.suffix.result}"
  resource_group_name           = azurerm_resource_group.main.name
  location                      = azurerm_resource_group.main.location
  version                       = "16"
  administrator_login           = var.postgres_admin_username
  administrator_password        = random_password.postgres_admin.result
  storage_mb                    = var.postgres_storage_mb
  sku_name                      = var.postgres_sku
  zone                          = "1"
  public_network_access_enabled = true

  lifecycle {
    ignore_changes = [zone]
  }
}

resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = var.db_name
  server_id = azurerm_postgresql_flexible_server.main.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

# Lets Azure App Service (which has no fixed outbound IP on Basic tier) reach
# the DB. Scoped to Azure's own backbone, not the open internet.
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_operator" {
  name             = "AllowTerraformOperator"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = trimspace(data.http.operator_ip.response_body)
  end_ip_address   = trimspace(data.http.operator_ip.response_body)
}

provider "postgresql" {
  host      = azurerm_postgresql_flexible_server.main.fqdn
  port      = 5432
  database  = azurerm_postgresql_flexible_server_database.main.name
  username  = var.postgres_admin_username
  password  = random_password.postgres_admin.result
  sslmode   = "require"
  superuser = false
}

# Least-privilege role split: `migrator` owns/creates schema objects (used only
# by the CI/CD migration step), `app` gets DML + EXECUTE only (used by the
# deployed API at runtime, no DDL rights).
resource "postgresql_role" "migrator" {
  name     = "eligibility_migrator"
  login    = true
  password = random_password.migrator_role.result

  depends_on = [azurerm_postgresql_flexible_server_firewall_rule.allow_operator]
}

resource "postgresql_role" "app" {
  name     = "eligibility_app"
  login    = true
  password = random_password.app_role.result

  depends_on = [azurerm_postgresql_flexible_server_firewall_rule.allow_operator]
}

resource "postgresql_grant" "migrator_database" {
  database    = azurerm_postgresql_flexible_server_database.main.name
  role        = postgresql_role.migrator.name
  object_type = "database"
  privileges  = ["CREATE", "CONNECT"]
}

resource "postgresql_grant" "migrator_schema" {
  database    = azurerm_postgresql_flexible_server_database.main.name
  role        = postgresql_role.migrator.name
  schema      = "public"
  object_type = "schema"
  privileges  = ["CREATE", "USAGE"]
}

resource "postgresql_grant" "app_database" {
  database    = azurerm_postgresql_flexible_server_database.main.name
  role        = postgresql_role.app.name
  object_type = "database"
  privileges  = ["CONNECT"]
}

resource "postgresql_grant" "app_schema" {
  database    = azurerm_postgresql_flexible_server_database.main.name
  role        = postgresql_role.app.name
  schema      = "public"
  object_type = "schema"
  privileges  = ["USAGE"]
}

# Objects don't exist yet at apply time (EF Core migrations create them later,
# running as `migrator`) — default privileges make sure `app` automatically
# gets the right grants on anything `migrator` creates from now on.
resource "postgresql_default_privileges" "app_tables" {
  role        = postgresql_role.app.name
  database    = azurerm_postgresql_flexible_server_database.main.name
  schema      = "public"
  owner       = postgresql_role.migrator.name
  object_type = "table"
  privileges  = ["SELECT", "INSERT", "UPDATE", "DELETE"]
}

resource "postgresql_default_privileges" "app_sequences" {
  role        = postgresql_role.app.name
  database    = azurerm_postgresql_flexible_server_database.main.name
  schema      = "public"
  owner       = postgresql_role.migrator.name
  object_type = "sequence"
  privileges  = ["USAGE", "SELECT", "UPDATE"]
}

resource "postgresql_default_privileges" "app_functions" {
  role        = postgresql_role.app.name
  database    = azurerm_postgresql_flexible_server_database.main.name
  schema      = "public"
  owner       = postgresql_role.migrator.name
  object_type = "function"
  privileges  = ["EXECUTE"]
}

# --- Frontend -----------------------------------------------------------

resource "azurerm_static_web_app" "frontend" {
  name                = "${var.name_prefix}-web-${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku_tier            = "Free"
  sku_size            = "Free"
}

# --- Backend --------------------------------------------------------------

resource "azurerm_service_plan" "main" {
  name                = "${var.name_prefix}-plan"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = var.app_service_sku
}

resource "azurerm_linux_web_app" "api" {
  name                = "${var.name_prefix}-api-${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_service_plan.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version = "10.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"     = "Production"
    "FrontendOrigin"             = "https://${azurerm_static_web_app.frontend.default_host_name}"
    "ConnectionStrings__Default" = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.app_db_connection.versionless_id})"
  }
}

# --- Secrets ----------------------------------------------------------------

resource "azurerm_key_vault" "main" {
  name                       = "${var.name_prefix}-kv-${random_string.suffix.result}"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  rbac_authorization_enabled = true
}

# RBAC-based access (the modern model — access policies are legacy). The
# deployer needs write access to seed the secret; the API's managed identity
# only needs read access at runtime.
resource "azurerm_role_assignment" "deployer_kv_officer" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azurerm_client_config.current.object_id
}

resource "azurerm_role_assignment" "api_app_kv_reader" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_web_app.api.identity[0].principal_id
}

resource "azurerm_key_vault_secret" "app_db_connection" {
  name         = "app-db-connection-string"
  key_vault_id = azurerm_key_vault.main.id
  value        = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${postgresql_role.app.name};Password=${random_password.app_role.result};Sslmode=Require"

  depends_on = [azurerm_role_assignment.deployer_kv_officer]
}

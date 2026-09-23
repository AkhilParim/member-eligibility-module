variable "resource_group_name" {
  description = "Name of the Azure resource group to create."
  type        = string
  default     = "rg-member-eligibility"
}

variable "location" {
  description = "Azure region. Must support Static Web Apps (e.g. eastus2, centralus, westus2, westeurope, eastasia) — and, for this subscription specifically, must not be one of the regions Azure restricts new/trial subscriptions from provisioning Postgres Flexible Server in (eastus2, eastus, westus2, westeurope, southcentralus all came back restricted; centralus, northeurope, eastasia, westus3, canadacentral, uksouth, australiaeast, japaneast, southeastasia did not)."
  type        = string
  default     = "centralus"
}

variable "name_prefix" {
  description = "Prefix used to build globally-unique resource names (App Service, Key Vault, Static Web App, Postgres server)."
  type        = string
  default     = "eligibility"
}

variable "app_service_sku" {
  description = "App Service Plan SKU for the API."
  type        = string
  default     = "B1"
}

variable "postgres_sku" {
  description = "Azure Database for PostgreSQL Flexible Server SKU."
  type        = string
  default     = "B_Standard_B1ms"
}

variable "postgres_storage_mb" {
  description = "Postgres Flexible Server storage size in MB."
  type        = number
  default     = 32768
}

variable "postgres_admin_username" {
  description = "Postgres server admin login (used only for initial provisioning, not by the app)."
  type        = string
  default     = "pgadmin"
}

variable "db_name" {
  description = "Application database name."
  type        = string
  default     = "member_eligibility"
}

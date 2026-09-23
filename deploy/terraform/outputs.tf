output "api_url" {
  value = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "frontend_url" {
  value = "https://${azurerm_static_web_app.frontend.default_host_name}"
}

output "static_web_app_deployment_token" {
  value     = azurerm_static_web_app.frontend.api_key
  sensitive = true
}

output "app_service_name" {
  value = azurerm_linux_web_app.api.name
}

output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "postgres_fqdn" {
  value = azurerm_postgresql_flexible_server.main.fqdn
}

output "migrator_connection_string" {
  value     = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${postgresql_role.migrator.name};Password=${random_password.migrator_role.result};Sslmode=Require"
  sensitive = true
}

output "app_connection_string" {
  value     = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${postgresql_role.app.name};Password=${random_password.app_role.result};Sslmode=Require"
  sensitive = true
}

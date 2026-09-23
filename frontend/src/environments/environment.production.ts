// apiBaseUrl is substituted by .github/workflows/deploy.yml with the real
// Azure App Service URL (only known after `terraform apply`) before `ng build`.
export const environment = {
  apiBaseUrl: '__API_BASE_URL__/api',
};

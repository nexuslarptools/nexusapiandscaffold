# Nexus API and Scaffold: Code Cleanup, Security, and Best Practices Recommendations

This document summarizes prioritized, actionable improvements for the repository across security, reliability, maintainability, and operability. Items are grouped and include rationale and concrete steps.

This report was produced from a SonarLint-guided static review in Rider on 2025-10-17. The items below map to common Sonar rules where applicable (examples: csharpsquid:S2068, csharpsquid:S5332, csharpsquid:S1075) and are prioritized for security and maintainability.

Note: Minimal, low-risk code/config changes were applied as part of this review:
- Sanitized appsettings.json to remove committed Auth0 secrets; replaced with environment-driven placeholders.
- Fixed malformed Auth0 UserInfoEndpoint URL (missing colon after https).

## 1) Critical Security

- Secrets management
  - Do not commit secrets to VCS. Remove all real secrets from appsettings.*.json. Use environment variables, Azure Key Vault/AWS Secrets Manager, or .NET Secret Manager in development.
  - Add a baseline secrets detection tool (e.g., Gitleaks) in CI.
- JWT/Auth configuration
  - Validate these mandatory config keys at startup: Auth0:Domain, Auth0:Audience/ApiIdentifier. Fail fast on missing values.
  - Use TokenValidationParameters.ValidateAudience = true; ValidateIssuer = true; ValidateLifetime = true; ValidateIssuerSigningKey = true (default in JwtBearer but ensure not relaxed).
- HTTPS everywhere
  - Enforce HTTPS redirection (UseHttpsRedirection) and HSTS in production. Avoid binding HTTP to port 443.
  - Terminate TLS at a reverse proxy or configure Kestrel certificates properly; never serve HTTPS over HTTP.
- CORS
  - In production, restrict origins to a config-driven allowlist. Avoid AllowAnyOrigin with credentials. Prefer using configuration arrays (e.g., Cors:AllowedOrigins) and environment-specific overrides.
- Headers / hardening
  - Add security headers middleware: X-Content-Type-Options, X-Frame-Options/Frame-ancestors (CSP), Referrer-Policy, Permissions-Policy, X-XSS-Protection (legacy), and strict Content-Security-Policy.
- MinIO/S3 credentials and endpoint
  - Only provide via environment/config. Support IAM roles or workload identity where possible. Avoid hardcoded endpoint URLs; make them configurable.

## 2) Configuration & Environment

- Configuration validation
  - Bind strongly-typed options for critical sections (Auth0, DB, MinIO) and call services.PostConfigure<T>(Validate) or options.ValidateDataAnnotations() with ValidateOnStart().
- Connection strings
  - Use a single ConnectionStrings:NexusDB entry (correct typo from ConnectionSrings) and pass via env var DOTNET_ConnectionStrings__NexusDB or standard ASP.NET env config.
  - Avoid logging connection strings; if necessary, redact passwords.
- App URLs
  - In Program, avoid UseUrls("http://*:80;http://*:443"). If behind a reverse proxy, use proper forwarding headers and only expose required ports. Configure Kestrel or the container orchestrator to bind ports.

## 3) API Surface & Behavior

- Swagger / OpenAPI
  - Security scheme for Bearer is present; ensure it uses HTTP scheme bearer with JWT format (type: http, scheme: bearer, bearerFormat: JWT) rather than apiKey for best tooling compatibility.
  - Include XML comments for better docs (enable in csproj and c.IncludeXmlComments).
- Versioning
  - Introduce API versioning (Microsoft.AspNetCore.Mvc.Versioning) and document versions in Swagger.
- Model validation
  - Ensure [ApiController] is applied to controllers, automatic 400 on validation errors, and actionable problem details.
- Error handling
  - Add a centralized exception handling middleware returning ProblemDetails (RFC 7807). Avoid leaking stack traces in production.

## 4) Authentication & Authorization

- Policies & roles
  - Replace direct role claim type strings with a centralized authorization configuration. Define policy-based authorization with named policies.
- Minimal token surface
  - Avoid storing tokens server-side unless necessary (options.SaveToken = false unless a feature requires it).

## 5) Observability

- OpenTelemetry
  - Ensure resource attributes include service.name, service.version from assembly, environment (deployment.environment). Avoid noisy console exporters in production.
  - Prefer OTLP exporter to a collector endpoint; keep Grafana Tempo/Loki/Prometheus integration via collector.
- Structured logging
  - Use structured logging with scopes and correlation IDs. Propagate trace IDs. Avoid logging PII or secrets.

## 6) Performance & Resilience

- Database
  - Use DbContext pooling (AddDbContextPool) when appropriate. Ensure proper lifetime and avoid long-lived contexts.
  - Configure Npgsql performance switches only as needed; review Legacy Timestamp behavior usage.
- HTTP & retries
  - Use IHttpClientFactory and Polly for retries, circuit breakers for outbound calls (e.g., MinIO or other services).
- Caching
  - Add response caching and/or distributed caching where applicable.

## 7) Docker & Runtime

- Container hardening
  - Use a non-root user in the final stage (RUN adduser ...; USER appuser). Limit file system permissions.
  - Avoid exposing 443 if TLS is not terminated in the container. Rely on reverse proxy or add certs properly.
  - Add HEALTHCHECK command.
  - Keep base images pinned to a digest or minor version and patch regularly.
- Environment variables
  - Document required env vars for deployment (Auth0, DB, MinIO, OTEL). Provide a sample.env file (without secrets).

## 8) CI/CD & Automation

- GitHub Actions
  - Ensure secrets are fetched via GitHub secrets and not echoed in logs. Limit token permissions (GITHUB_TOKEN least privilege).
  - Add static analysis: dotnet format, analyzers, and security scans (CodeQL, Trivy for images, Gitleaks).
- Build quality gates
  - Enforce warnings as errors for your project. Add test and coverage thresholds if tests exist.

## 9) Code Cleanup & Style

- Target .NET 9 best practices
  - Prefer endpoint routing over UseMvc; migrate to MapControllers with UseRouting/UseEndpoints.
  - Remove dead code and commented blocks that hold secrets or internal hosts. Use configuration or documentation instead.
- Analyzers
  - Enable nullable reference types, add Roslyn analyzers, StyleCop/EditorConfig to standardize style.

## 10) Concrete To-Do Checklist

- [ ] Replace Program.UseUrls with environment/Kestrel configuration; remove HTTP on 443.
- [ ] Add UseHttpsRedirection and ensure HSTS only in production.
- [ ] Add HealthChecks (services.AddHealthChecks(); app.MapHealthChecks("/health")) after migrating to endpoint routing.
- [ ] Configure CORS via configuration arrays (Cors:AllowedOrigins) and remove AllowAnyOrigin+Credentials combos.
- [ ] Bind Options: Auth0Options, DatabaseOptions, StorageOptions with validation; ValidateOnStart.
- [ ] Switch Swagger security to type http, scheme bearer, bearerFormat JWT; add XML comments.
- [ ] Remove secrets from repo history (git filter-repo or BFG) and rotate compromised credentials (Auth0, MinIO).
- [ ] Add non-root container user and HEALTHCHECK to Dockerfile.
- [ ] Add static security tools to CI (CodeQL, Gitleaks, Trivy) and dotnet analyzers.

By prioritizing the above, you will significantly improve security, operational resilience, and maintainability with minimal risk.


## Reverse proxy (Traefik) deployment notes

- This API is designed to run behind a reverse proxy that terminates TLS (Traefik). The app processes X-Forwarded-For and X-Forwarded-Proto to preserve client IP and original scheme and to avoid HTTPS redirect loops.
- Configure the proxy IPs so forwarded headers are trusted in Production:
  - Environment variable: ReverseProxy__KnownProxies=10.0.0.10,10.0.0.11
  - Or appsettings (Production):
    - ReverseProxy:
      - KnownProxies: ["10.0.0.10", "10.0.0.11"]
- Ensure Traefik forwards the headers X-Forwarded-For and X-Forwarded-Proto to the application service.
- Keep UseHttpsRedirection and HSTS enabled; with forwarded headers, the app respects the original https scheme from Traefik.


### Observability updates (2025-10-17)

- OpenTelemetry resource enriched with:
  - service.name=NEXUSDataLayerScaffold (from assembly)
  - service.version (from assembly)
  - service.namespace=NEXUS
  - service.instance.id (machine name)
  - deployment.environment (from ASPNETCORE_ENVIRONMENT)
- Console logging is enabled only in Development to avoid noisy logs in Production; OpenTelemetry logging exporter remains enabled across environments.
- Recommended environment variables for production deployments:
  - OTEL_EXPORTER_OTLP_ENDPOINT=https://otel-collector:4317
  - OTEL_RESOURCE_ATTRIBUTES=service.version=1.2.3,deployment.environment=Production (optional override)
  - ASPNETCORE_ENVIRONMENT=Production
- Tracing and metrics are exported via the Grafana.OpenTelemetry package (OTLP to a collector). Prefer using a collector to route traces/metrics to Tempo/Prometheus and logs to Loki.


### Code cleanup & style updates (2025-10-17)

- Adopted .NET 9 endpoint routing (UseRouting/UseEndpoints with MapControllers already in place); removed legacy MVC registrations.
- Removed stale commented code blocks (old OpenIdConnect/Auth samples, internal UseUrls hints, and legacy Swagger stub) to avoid accidental leakage and reduce noise.
- Enabled nullable reference types and .NET analyzers:
  - csproj: <Nullable>enable</Nullable>, <EnableNETAnalyzers>true</EnableNETAnalyzers>, <AnalysisLevel>latest</AnalysisLevel>
  - Added a root .editorconfig with baseline C# conventions and non-invasive analyzer severities.
- Next optional hardening (not done now to keep changes minimal): add StyleCop.Analyzers and project-specific rules; consider gradually elevating analyzer severities and treating warnings as errors once the codebase is clean.

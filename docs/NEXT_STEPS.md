# Next Steps Plan

This document outlines prioritized, actionable next steps for the Nexus API codebase. It is designed for incremental adoption with minimal disruption. Update owners and due dates as you assign work.

Last updated: 2025-11-15 09:13 local

Legend:
- P0 = Highest priority (security/stability)
- P1 = Important (performance/maintainability)
- P2 = Nice-to-have (DX/observability)

---

## P0 — Security and Configuration Hygiene

- [ ] Externalize MinIO configuration (Endpoint, UseSsl, Bucket, Region) and credentials
  - Owner: TBD
  - Steps:
    - Add configuration section `"Minio": { "Endpoint": "", "UseSsl": true, "Bucket": "", "Region": "" }` to appsettings with non-secret defaults.
    - Source AccessKey/SecretKey from environment variables or secret store (do not commit real secrets).
    - Update DI registration in Startup.cs to read from configuration instead of hardcoded `WithEndpoint("decade.kylebrighton.com:9000")`; call `.WithSSL()` conditionally when UseSsl = true.
  - Acceptance criteria:
    - Application starts with MinIO settings entirely controlled via configuration.
    - No secrets present in repo or logs.
  - Target date: YYYY-MM-DD

- [ ] Verify and enforce OAuth/Auth0 settings
  - Owner: TBD
  - Steps:
    - Confirm authority/audience come from either OAuth or Auth0 sections consistently.
    - Add scope/role checks on sensitive endpoints where applicable.
  - Acceptance criteria: Requests without required scopes are rejected with 403; OpenAPI notes required scopes where applicable.
  - Target date: YYYY-MM-DD

---

## P1 — Performance and Maintainability

- [ ] Add AsNoTracking to read-only queries and project to lightweight DTOs
  - Owner: TBD
  - Steps:
    - Audit GET endpoints in CharacterSheetsController and CharacterSheetApprovedsController.
    - Apply `.AsNoTracking()` to read queries; switch to `Select` projections to DTOs to avoid loading large graphs.
  - Acceptance criteria: 20–40% reduction in DB materialization time on typical GETs (measure locally with tracing/metrics).
  - Target date: YYYY-MM-DD

- [ ] Introduce server-side pagination with sane limits
  - Owner: TBD
  - Steps:
    - Add `page` and `pageSize` parameters to list/search endpoints; default pageSize 25, max 100.
    - Ensure deterministic ordering; validate parameters.
  - Acceptance criteria: Endpoints return paged results; large lists no longer produce timeouts or huge payloads.
  - Target date: YYYY-MM-DD

- [ ] Begin controller slim-down by extracting application services
  - Owner: TBD
  - Steps:
    - Create `Logic/CharacterSheets/CharacterSheetQueryService` and move the hottest query logic there.
    - Unit-test the service in isolation.
  - Acceptance criteria: At least one high-traffic endpoint uses the new service; controller code for that endpoint shrinks by >40%.
  - Target date: YYYY-MM-DD

- [ ] Model nullability hygiene
  - Owner: TBD
  - Steps:
    - Decide on semantics for `ItemSheetReviewMessage`:
      - Make `Message` required or mark as `string?` if DB permits null.
      - Convert `Isactive` to non-nullable `bool` with default if tri-state not needed.
      - Consider `DateTimeOffset` for `Createdate` and ensure UTC handling.
    - Align EF model and DB schema (migration if needed).
  - Acceptance criteria: No nullability warnings; runtime null refs eliminated on these properties; DB schema consistent.
  - Target date: YYYY-MM-DD

---

## P1 — Error Handling and API Consistency

- [ ] Standardize ProblemDetails for errors and validation
  - Owner: TBD
  - Steps:
    - Introduce ProblemDetails factory and middleware or filters to return RFC7807 consistently.
    - Ensure validation errors return 400 with traceId included.
  - Acceptance criteria: All errors adhere to ProblemDetails; integration tests cover validation and 500 paths.
  - Target date: YYYY-MM-DD

---

## P2 — Developer Experience and Observability

- [ ] Swagger/OpenAPI improvements
  - Owner: TBD
  - Steps:
    - Enable Bearer/JWT security scheme and apply to protected endpoints.
    - Add XML comments; annotate response types.
  - Acceptance criteria: Auth works via Swagger UI for testing; models and responses are documented.
  - Target date: YYYY-MM-DD

- [ ] Telemetry hygiene and custom metrics
  - Owner: TBD
  - Steps:
    - Ensure no PII is logged.
    - Add custom metrics for hot endpoints (request duration, DB query count) via OpenTelemetry.
  - Acceptance criteria: Key metrics visible in Grafana with tags for endpoint and status code.
  - Target date: YYYY-MM-DD

---

## P2 — CI/CD and Testing

- [ ] Introduce tests and run in CI
  - Owner: TBD
  - Steps:
    - Create a test project with at least:
      - 1–2 unit tests for new services.
      - 1–2 integration tests for critical endpoints.
    - Update Bitbucket pipeline to run `dotnet restore`, `dotnet build`, `dotnet test` before Docker build.
  - Acceptance criteria: CI fails on test failures; code coverage reported (optional at first pass).
  - Target date: YYYY-MM-DD

---

## Rollout and Risk Management

- Start with configuration changes behind environment variables; deploy to a non-production environment first.
- Use feature flags or environment toggles for new pagination defaults if front-end assumptions exist.
- Monitor traces/metrics after each change; have a quick rollback path (previous image tag) available.

---

## Quick Wins (suggested order)

1) Externalize MinIO config (P0)
2) Add AsNoTracking + pagination to top 1–2 GET endpoints (P1)
3) Fix `ItemSheetReviewMessage` nullability (P1)

---

## Change Log

- 2025-11-15: Initial version created.

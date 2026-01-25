You are a senior F# SAFE Stack solution architect and product engineer.

Goal
Design a production-ready full-stack web application using the SAFE Stack:
- Saturn (server on ASP.NET Core/Giraffe foundation)
- Azure App Service (hosting target)
- Fable (F# -> JavaScript)
- Elmish (MVU architecture on React)
Assume we will start from the official SAFE dotnet template (SAFE.Template) and follow SAFE docs conventions. Include Vite-based client dev workflow and a pinned .NET SDK via global.json; use Fantomas for formatting. Cite any SAFE-specific assumptions in your output. 
(References: SAFE docs + SAFE template readme; do not invent template details you cannot justify.) 

Context (fill in)
- Product domain: <DOMAIN>
- Target users/personas: <PERSONAS>
- Core jobs-to-be-done: <JTBD>
- Key screens/pages: <PAGES>
- Data entities: <ENTITIES>
- Integrations (if any): <INTEGRATIONS>
- Auth requirements: <AUTH none/basic/OIDC/etc.>
- Hosting constraints: Azure App Service yes/no, other Azure services: <SERVICES>
- Non-functional requirements: <SLO/perf/security/compliance>
- Team constraints: <TEAM_SIZE>, <SKILL_LEVEL>, <TIMELINE>, <BUDGET>

Deliverables (produce all sections)
1) Executive summary
   - Problem statement, target outcomes, success metrics, MVP scope.

2) SAFE-aligned architecture
   - Solution structure: src/Server, src/Client, src/Shared (or template-equivalent).
   - Shared code strategy: which domain types live in Shared; serialization strategy; versioning.
   - Client: Elmish MVU structure (Model/Msg/Update/View), routing approach, state management conventions.
   - Server: Saturn route structure, API boundaries, error handling contract, validation strategy.
   - Client/server communication:
     - Default: REST/HTTP endpoints with typed DTOs in Shared
     - Optional: evaluate typed RPC via Fable Remoting (if appropriate) and justify.
   - Local dev loop: describe commands and workflow aligned with SAFE docs (Fable watch + Vite).
   - Testing strategy: server tests, client tests, contract tests; what is feasible in SAFE tooling.

3) Technical decisions (ADR-style)
   For each decision, provide: context, options, decision, consequences.
   - UI approach (e.g., Feliz vs other SAFE-compatible approach if relevant)
   - Styling strategy (e.g., Tailwind optional)
   - State/routing strategy
   - Data persistence (if any): options, recommendation
   - Logging/observability (Azure-friendly)
   - CI/CD approach for Azure App Service

4) API specification
   - List endpoints (or RPC methods), request/response types, status codes, error schema.
   - Authentication/authorization rules per endpoint.
   - Validation rules and examples.
   - Backward compatibility/versioning plan.

5) Data model specification
   - Core entities and relationships.
   - Storage schema (if applicable) and migration approach.
   - Sample data sets for dev/test.

6) Security and compliance checklist
   - Threat model summary (top risks + mitigations)
   - Secrets management approach (Azure)
   - Dependency and supply-chain hygiene
   - Privacy considerations if personal data exists

7) Delivery plan and backlog
   - MVP milestones (2–4 iterations)
   - A prioritized backlog of user stories with acceptance criteria (Gherkin)
   - Risks, dependencies, and “spikes” needed to de-risk items

8) “Getting started” instructions
   - Exact steps to scaffold using the SAFE dotnet template (dotnet new SAFE template usage at a high level)
   - Tooling prerequisites (Node/NPM for Vite; .NET SDK pinning; Fantomas)
   - Local run workflow and troubleshooting tips

Output format requirements
- Use clear headings, bullet lists, and tables where helpful.
- If you are uncertain about a SAFE-template default, label it explicitly as “uncertain” and provide a safe alternative.
- Do not provide unverified claims about current versions; keep versions as placeholders unless provided as input.
- Keep the result actionable: someone should be able to start implementation from your spec without further clarification.

Inputs
Here is any additional information you must incorporate:
<PASTE NOTES / EXISTING REQUIREMENTS / CONSTRAINTS HERE>

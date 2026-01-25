Core delivery roles
Product Owner (PO) — prompts

Define outcome and success metrics

“Act as a Product Owner. Given this product context: [context], define the next 1–2 sprint goals as outcomes, not tasks. Propose 3–5 measurable success metrics (leading + lagging), and state baseline vs. target.”

Backlog refinement and prioritization

“Act as a Product Owner. Here is the backlog list with rough notes: [items]. Rewrite each as a user story with acceptance criteria (Gherkin), assign a business value score (1–10), risk score (1–10), and propose an ordered backlog with justification.”

Sprint planning input

“Act as a Product Owner. Using this ordered backlog and constraints: [backlog + constraints], propose a sprint scope that maximizes value while managing risk. Include: sprint goal, in-scope items, out-of-scope items, and tradeoffs.”

Stakeholder alignment brief

“Act as a Product Owner. Draft a 1-page stakeholder update for [audience] covering: goal, what shipped, what changed, open decisions, risks, and what you need from stakeholders.”

Release decision

“Act as a Product Owner. Based on these test results, known issues, and business timeline: [details], recommend ship/hold. If ship: propose comms and mitigations. If hold: propose the minimum gating criteria to resume.”

Scrum Master — prompts

Impediment removal and escalation

“Act as a Scrum Master. From these blockers: [list], categorize by type (team/process/dependency/tech/people), propose a removal plan with owners and deadlines, and draft an escalation note for leadership for any blocker >48h.”

Facilitate sprint planning

“Act as a Scrum Master. Create a 60–90 minute sprint planning agenda tailored to this team: [team info], including prompts to elicit a sprint goal, confirm capacity, and define a clear ‘done’ boundary.”

Daily Scrum improvement

“Act as a Scrum Master. Given these recurring issues in Daily Scrum: [issues], propose a new structure (questions, board focus, timeboxing) and 2 experiments to run next sprint with measurable indicators.”

Retrospective facilitation

“Act as a Scrum Master. Generate a retrospective plan (45/60/90 min options) using a specific format (e.g., Start/Stop/Continue or 4Ls). Include: warm-up, data gathering, insights, 2 improvement actions with owners, and follow-up checks.”

Team working agreements

“Act as a Scrum Master. Create a draft ‘team working agreement’ for: PR reviews, WIP limits, definition of ready/done, meeting norms, and on-call/interrupt handling. Keep it concise and enforceable.”

Developers (cross-functional) — prompts

Break down a story into implementable tasks

“Act as a developer team. For this user story and acceptance criteria: [story], propose a technical approach and break it into tasks (1–2 day slices). Identify dependencies, risks, and test strategy.”

Definition of Done checklist

“Act as a developer team. Create a Definition of Done checklist for our product type: [web/mobile/data/etc.]. Include code, tests, security, docs, performance, observability, and release steps.”

Technical spike plan

“Act as a developer team. We need to de-risk: [topic]. Propose a timeboxed spike plan (max 1–2 days), hypotheses, evaluation criteria, and a recommendation template for the outcome.”

Code review quality rubric

“Act as a senior engineer. Draft a code review rubric with severity levels (nit/minor/major/blocker) covering readability, correctness, security, performance, testing, and maintainability.”

Incident / bug triage

“Act as a developer team. Given this bug report: [details], triage it: reproduce steps, suspected root causes, impact/severity, immediate mitigation, and a fix plan with test coverage.”

Common specialist roles inside an Agile team
Software Engineer / Developer — prompts

“Act as a [frontend/backend/mobile/platform] engineer. Design the solution for: [feature]. Provide: architecture sketch (text), key components, API/contracts, data model changes, and an implementation plan.”

“Given this code or pseudo-code: [snippet], identify defects, edge cases, and propose improvements with reasoning.”

QA / Test Engineer — prompts

“Act as a QA engineer. For this story: [story], create a test plan: functional cases, negative cases, exploratory charters, automation candidates, and test data needs.”

“Create acceptance test scenarios in Gherkin for: [feature]. Cover boundary conditions and failure modes.”

UX/UI Designer — prompts

“Act as a UX/UI designer. For user persona(s): [personas] and goal: [goal], propose 2–3 user flows with key screens, primary CTAs, error states, and accessibility considerations.”

“Create a lightweight design critique checklist for this UI: [description/screens], covering hierarchy, consistency, states, and usability risks.”

UX Researcher — prompts

“Act as a UX researcher. Propose a study plan to validate: [assumption]. Include research questions, method (interviews/usability test/survey), recruiting criteria, script outline, and how we’ll synthesize results.”

“Given these notes/transcripts: [notes], synthesize themes, key quotes (short), insights, and prioritized recommendations.”

Business Analyst — prompts

“Act as a business analyst. For this business problem: [problem], map stakeholders, current process, pain points, and propose a target process with clear requirements and non-functional constraints.”

“Convert these stakeholder notes: [notes] into a requirements pack: user stories, acceptance criteria, business rules, and open questions.”

Architect / Solution Architect — prompts

“Act as a solution architect. For this initiative: [initiative], propose architecture options (2–3), with tradeoffs across security, scalability, cost, maintainability, and delivery speed. Recommend one with rationale.”

“Create an ADR (Architecture Decision Record) for decision: [decision]. Include context, options, decision, consequences, and follow-up tasks.”

DevOps / SRE — prompts

“Act as an SRE. For service: [service], propose an observability plan: SLIs/SLOs, dashboards, alerts, log structure, and runbook outline.”

“Design a CI/CD pipeline for: [repo/service]. Include build/test/security scanning, environments, promotion rules, rollback strategy, and required secrets handling.”

Data Engineer / Data Scientist / ML Engineer — prompts

“Act as a data engineer. For this data source/need: [details], propose a pipeline design: ingestion, validation, schema, transformations, storage, lineage, and monitoring.”

“Act as an ML engineer. For model objective: [objective], propose features, evaluation metrics, experiment plan, baseline, and deployment considerations (drift, retraining, monitoring).”

Technical Writer — prompts

“Act as a technical writer. Draft user-facing documentation for: [feature]. Include overview, step-by-step, screenshots placeholders, troubleshooting, and FAQs. Keep language simple and precise.”

“Create release notes for version [x]. Inputs: [changes]. Output: customer-impact summary, new features, fixes, known issues, and upgrade notes.”
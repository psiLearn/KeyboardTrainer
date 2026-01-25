# KeyboardTrainer Agent Instructions

You are a **multi-role AI agent** guiding the KeyboardTrainer project through an integrated Agile workflow using the SAFE Stack (F#).

---

## 1. Your Mission

Help teams **define, build, and deliver** a production-ready **KeyboardTrainer** web application — a typing learning app using a German QWERTZ keyboard layout for visualization and French content for practice.

By:
- Facilitating Agile ceremonies and role responsibilities
- Applying SAFE Stack architectural principles (Saturn server, Fable/Elmish client, PostgreSQL persistence, Docker Compose local dev)
- Maintaining alignment between product strategy and technical execution

**Project Context:** See [app.md](app.md) for the complete product specification.

---

## 2. Operating Framework

### Phase I: Discovery and Product Strategy
**Owner: Product Owner (PO)**

**Inputs:**
- User personas and jobs-to-be-done (from workflow.md)
- Product domain, key screens, data entities
- Hosting constraints and non-functional requirements

**Apply these PO prompts from Roles.md:**
- **Define outcome and success metrics**: Establish 1–2 sprint goals as outcomes, 3–5 measurable success metrics (leading + lagging), baseline vs. target.
- **Backlog refinement and prioritization**: Convert rough notes into user stories with Gherkin acceptance criteria, business/risk scores, ordered backlog.
- **Stakeholder alignment brief**: 1-page update covering goal, scope, open decisions, risks, stakeholder needs.

**Deliverable:** Executive summary + ordered product backlog with clear success metrics.

---

### Phase II: Architecture and Technical Design
**Owner: Solution Architect (leveraging Architect role from Roles.md)**

**Inputs:**
- Product strategy from Phase I
- SAFE Stack template defaults (from techstack.md)
- Team constraints (size, skill level, timeline, budget)

**Apply:**
- **techstack.md framework**: Generate 8 architectural deliverables:
  1. Executive summary
  2. SAFE-aligned architecture (server/client/shared structure, API, MVU patterns, local dev loop)
  3. Technical decisions (ADR-style: UI approach, styling, state/routing, data persistence, logging, CI/CD)
  4. API specification (endpoints, auth rules, validation, versioning)
  5. Data model specification (entities, schema, migrations, sample data)
  6. Security and compliance checklist (threat model, secrets, privacy)
  7. Delivery plan and backlog (MVP milestones, prioritized stories, risks/spikes)
  8. Getting started instructions (scaffolding, tooling, local run workflow)

**Deliverable:** Production-ready technical specification (TechSpec) ready for development.

---

### Phase III: Sprint Planning and Backlog Refinement
**Owners: Scrum Master (SM) + Product Owner (PO) + Developers**

**Apply these prompts from Roles.md:**

**PO:**
- **Sprint planning input**: Propose sprint scope from ordered backlog + constraints; include sprint goal, in-scope/out-of-scope items, tradeoffs.

**SM:**
- **Facilitate sprint planning**: Create 60–90 minute agenda with sprint goal elicitation, capacity confirmation, clear DoD.
- **Team working agreements**: Draft agreement for PR reviews, WIP limits, definition of ready/done, meeting norms, on-call handling.

**Developers:**
- **Break down a story into implementable tasks**: For each story, propose technical approach, 1–2 day task slices, identify dependencies/risks/test strategy.
- **Definition of Done checklist**: Create DoD for web product (code, tests, security, docs, performance, observability, release).

**Deliverable:** Sprint backlog with concrete, implementable tasks + team working agreement.

---

### Phase IV: Build and Validate Increment
**Owner: Agile Team (cross-functional developers, QA, UX, specialists)**

**Workflow (from workflow.md):** Build and Validate Increment → Review and Demo

**Apply these prompts from Roles.md:**

**Developers:**
- **Code review quality rubric**: Use severity levels (nit/minor/major/blocker) covering readability, correctness, security, performance, testing, maintainability.
- **Incident / bug triage**: For blockers, reproduce steps, root causes, severity, immediate mitigation, fix plan + test coverage.

**QA / Test Engineer:**
- **Test plan creation**: For each story, create functional cases, negative cases, exploratory charters, automation candidates, test data needs.
- **Acceptance test scenarios**: Write Gherkin scenarios covering boundary conditions and failure modes.

**UX/UI Designer:**
- **Design critique checklist**: Review hierarchy, consistency, states, usability risks for delivered screens.

**Engineers (specialists):**
- **Software Engineer**: Design solution with architecture sketch, key components, API/contracts, data model changes.
- **UX Researcher** (if needed): Validate assumptions with study plan or synthesize user research.
- **Business Analyst**: Map stakeholders, current process, pain points; propose target process with clear requirements.

**Deliverable:** Working increment meeting Definition of Done + passing all acceptance tests.

---

### Phase V: Review, Demo, and Release Decision
**Owner: Product Owner (PO)**

**Apply these PO prompts from Roles.md:**
- **Release decision**: Based on test results, known issues, business timeline: recommend ship/hold. If ship: propose comms + mitigations. If hold: propose minimum gating criteria.

**Deliverable:** Release decision with mitigation plan (if needed).

---

### Phase VI: Measure Outcomes and Retrospective
**Owner: Scrum Master (SM) + Product Owner (PO) + Team**

**Apply these prompts from Roles.md:**

**SM:**
- **Retrospective facilitation**: Run retrospective (45/60/90 min) using Start/Stop/Continue or 4Ls format. Include: warm-up, data gathering, insights, 2 improvement actions with owners, follow-up checks.
- **Daily Scrum improvement**: Address recurring issues; propose new structure (questions, board focus, timeboxing) and 2 experiments for next sprint.

**PO:**
- **Stakeholder alignment brief**: Update stakeholders on outcomes, what shipped, what changed, open decisions, new learnings.

**Deliverable:** Improvement actions + team learning → **Loop back to Phase II or Phase III for next iteration** (see workflow.md feedback loop).

---

## 3. How to Use These Instructions

### For a **Specific Phase**, follow this pattern:

1. **Identify the current phase** (Discovery → Architecture → Sprint Planning → Build → Review → Release → Retrospective)
2. **Check the "Apply" section** for the relevant role prompts
3. **Use those prompts verbatim or adapt them** to your context (substitute placeholders like `[story]`, `[team info]`, etc.)
4. **Deliver the named output** (e.g., executive summary, TechSpec, sprint backlog)
5. **Move to the next phase** when the deliverable is complete

### For **Impediment Removal**, use the SM prompt:
- "Act as a Scrum Master. From these blockers: [list], categorize by type (team/process/dependency/tech/people), propose a removal plan with owners and deadlines, and draft an escalation note for leadership for any blocker >48h."

### For **Cross-cutting Concerns** (security, testing, architecture decisions):
- Refer to **Phase II (Architecture and Technical Design)** sections 3, 5, 6.
- Use **Phase IV specialist prompts** for quality, UX, data validation.

---

## 4. Key Principles

1. **Role-based accountability**: Each phase has a clear owner (PO, SM, Developers, Specialists). Use Roles.md prompts to guide that owner.
2. **Workflow alignment**: Follow the workflow.md flow (Discovery → Strategy → Backlog → Planning → Build → Review → Release → Measure → Loop).
3. **SAFE Stack focus**: All technical decisions must align with SAFE architecture (Saturn/Azure/Fable/Elmish) and deliverables from techstack.md.
4. **Iterative delivery**: Work in 1–2 week sprints, with measurable outcomes and feedback loops after each increment.
5. **Agile ceremonies**: Sprint Planning, Daily Scrum, Sprint Review/Demo, Retrospective. Use SM prompts from Roles.md to run them effectively.

---

## 5. File Structure Reference

```
instructions/
├── Roles.md              ← Role-specific prompt templates
├── workflow.md           ← Agile workflow process diagram + phase sequence
├── techstack.md          ← SAFE Stack technical specification framework
└── AGENT_INSTRUCTIONS.md ← This file (integration guide)
```

---

## 6. Quick Start for Next Action

**If you are starting a new project:**
1. Fill in the context placeholders from techstack.md (domain, personas, JTBD, constraints).
2. Use the **Phase I** PO prompts to define product goals and success metrics.
3. Use the **Phase II** TechSpec framework to generate architecture.
4. Proceed through phases III–VI iteratively.

**If you are mid-project:**
1. Identify the current phase above.
2. Reference the corresponding "Apply" section.
3. Use the named prompt(s) from Roles.md.
4. Deliver the output.

**If you need to unblock a team:**
1. Use the **SM Impediment Removal prompt** from Roles.md.
2. Escalate if blockers exceed 48 hours.

---

## 7. Example: Applying the Framework

**Scenario: You are starting KeyboardTrainer and need to plan the first sprint.**

1. **Current phase:** Sprint Planning (Phase III)
2. **PO action:** Use "Sprint planning input" prompt:
   ```
   Act as a Product Owner. Using this ordered backlog and constraints: [YOUR BACKLOG + CONSTRAINTS], 
   propose a sprint scope that maximizes value while managing risk. 
   Include: sprint goal, in-scope items, out-of-scope items, and tradeoffs.
   ```
3. **SM action:** Use "Facilitate sprint planning" prompt:
   ```
   Act as a Scrum Master. Create a 60–90 minute sprint planning agenda tailored to this team: [YOUR TEAM INFO], 
   including prompts to elicit a sprint goal, confirm capacity, and define a clear 'done' boundary.
   ```
4. **Dev action:** Use "Break down a story into implementable tasks":
   ```
   Act as a developer team. For this user story and acceptance criteria: [STORY], 
   propose a technical approach and break it into tasks (1–2 day slices). 
   Identify dependencies, risks, and test strategy.
   ```
5. **Output:** Sprint plan with goal, scope, tasks, and team alignment.

---

## 8. Customization & Extensions

- **Add domain-specific roles** (e.g., DevOps, Data Engineer) by extending Roles.md.
- **Refine SAFE assumptions** in techstack.md as you learn team constraints.
- **Adapt workflow.md phases** if your delivery cadence differs (e.g., Kanban instead of Sprints).
- **Extend security/compliance** sections in Phase II if your domain requires it (healthcare, finance, etc.).

---

**Last Updated:** January 25, 2026  
**Project:** KeyboardTrainer (F# SAFE Stack)  
**Framework:** Agile + SAFE + Role-Based Agent Instructions

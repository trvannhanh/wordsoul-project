# AI Workflow Rules

## Approach

Build WordSoul incrementally with one accountable executor, one active branch, and evidence-based verification. These context files provide fast orientation; `WordSoulApi/docs/` remains authoritative for product decisions, module scope, Phase A tasks, REL/CT controls, gates, and evidence. Never invent behavior to fill a gap.

## Scoping Rules

- Work on one feature unit or one dependency-linked batch at a time.
- Prefer small, end-to-end verifiable increments over broad speculative rewrites.
- Do not combine unrelated API, learner-web, admin, and mobile concerns in one step.
- Use the current `vocamon-project` checkout and preserve unrelated user changes. Do not require a dedicated worktree or alias branch.
- A documentation task does not authorize source-code changes; implementation does not authorize product-scope changes.

## When to Split Work

Split an implementation step when it combines:

- A contract change with multiple clients that cannot be verified together quickly.
- Unrelated modules, routes, screens, migrations, workers, or providers.
- A security/data decision and implementation before acceptance criteria are clear enough for self-checking.
- More than one independent rollback boundary or Definition of Done.

If a change cannot be explained, tested, and safely reverted as one unit, split it.

## Handling Missing Requirements

- Make the smallest explicit product, legal, privacy, security, academic, or release decision needed for the current task and record material tradeoffs.
- Check `WordSoulApi/docs/02-modules/`, the Phase A import table, relevant slice, and REL/CT first.
- Record unresolved points under **Open Questions** in `progress-tracker.md` and in the canonical task/decision document when scope is affected.
- Use `Bị chặn` only for missing data, tools, or a real technical dependency; otherwise decide, document, and continue.

## Protected Files and Data

Do not modify unless the active task explicitly requires it:

- Applied EF migrations and migration history.
- Authentication, authorization, middleware order, rate limits, secret handling, or production configuration.
- Generated/dependency output: `node_modules/`, `bin/`, `obj/`, `dist/`, `.next/`, `.expo/`, coverage, and caches.
- Third-party library internals or generated UI code.

Never expose `.env` values, appsettings secrets, Firebase credentials, JWTs, refresh tokens, personal data, or raw provider payloads in prompts, logs, patches, screenshots, tests, docs, or evidence.

## Keeping Context in Sync

- Goals/scope → `project-overview.md`.
- Boundaries, stack, storage, auth, invariants → `architecture.md`.
- Enforced conventions and verification → `code-standards.md`.
- Tokens, libraries, layout, typography, interaction → `ui-context.md`.
- Execution policy → `ai-workflow-rules.md`.
- Current state, decisions, questions, resume point → `progress-tracker.md`.

Update canonical WordSoul docs first when a change affects business decisions, task scope/status, REL/CT, gates, or evidence.

## Before Moving to the Next Unit

1. The unit works end to end, including important denial/error/retry paths.
2. No architecture invariant or active CT/REL is violated.
3. Relevant tests, lint, type checks, and builds pass; record anything not run.
4. Canonical task status and `progress-tracker.md` reflect reality; WSA-7K2 may self-accept completion after the selected checks pass.
5. The diff contains no unrelated files, secrets, personal data, generated output, or accidental API/schema changes.

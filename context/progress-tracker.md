# Progress Tracker

This is optional session context, not a task tracker. Update it only when the overall goal, resume point, or project-level state changes. Canonical task status is in `WordSoulApi/docs/TASKS.md`.

## Current Phase

- Phase A preparation and controlled execution.
- Solo model active: WSA-7K2 owns 167 tasks / 412 weighted points.
- Canonical distribution: 167 `Chưa bắt đầu`, 0 `Đang thực hiện`, 0 `Bị chặn`, 0 `Hoàn thành`.

## Current Goal

- Execute the next dependency-correct task from `WordSoulApi/docs/TASKS.md`.

## Completed

- Consolidated WSA-7K2/WSA-9M4 into one executor, tracker, and work log.
- Removed the dedicated WSA-7K2 worktree; execution now uses the current repository checkout.
- Prepared initial Phase A drafts for REL-01/REL-02/CT-02, M01 identity/account/data/registration, and M11 administration/configuration/metrics.
- Created this repository-level context baseline from current manifests, source structure, UI tokens, and execution documents.
- Reduced active documentation to `PROJECT.md`, `DECISIONS.md`, and `TASKS.md`; Git now serves as the work log.

## In Progress

- No source-code task is currently marked `Đang thực hiện` by this context setup.
- Foundation drafts remain subject only to recorded technical/data dependencies and WSA-7K2 self-checks.

## Next Up

1. Choose one small dependency-correct batch from `WordSoulApi/docs/TASKS.md` and move it to `Đang thực hiện`.
2. Implement it and run boundary-appropriate verification.
3. Record the short check result in the task row, then self-accept and mark `Hoàn thành` when checks pass.

## Open Questions

- Which deployment environment/configuration baseline is authoritative for the first release candidate?
- What market/age/consent decision closes REL-01?
- Which provider capabilities are enabled for Phase A, and which remain held by CT/REL or deferred scope?
- Should web, admin, and mobile converge on shared semantic tokens or remain intentionally separate themes?

## Architecture Decisions

- Use .NET 9 layered/Clean Architecture: Domain → Application → Infrastructure → API composition.
- SQL Server is durable relational storage; Redis is cache/coordination; large assets use external media/blob storage.
- The API enforces identity, authorization, account state, ownership, and idempotency; clients provide no trusted domain truth.
- Maintain three client frameworks and visual languages rather than forcing shared runtime UI code.
- WSA-7K2 is the sole executor and decision maker; no separate specialist/release approval is required.

## Session Notes

- Active checkout: `D:\NhanhWorkspace\vocamon-project`.
- Active branch at workflow conversion: `feat/feature2`.
- Preserve unrelated existing changes in the checkout.
- Creating these Markdown files implies no build/test result; verify when implementation boundaries change.

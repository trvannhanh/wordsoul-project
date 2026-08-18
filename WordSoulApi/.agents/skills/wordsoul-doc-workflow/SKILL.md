---
name: wordsoul-doc-workflow
description: Read, update, and track WordSoul business documentation under the one-person workflow. Use for files under WordSoulApi/docs, task status, module backlog/decisions, optional evidence, REL/CT, gates, or execution planning. Do not use for source-code implementation unless separately requested.
---

# WordSoul documentation workflow — solo

## Establish context

1. Read `../../../docs/README.md`.
2. Read `../../../docs/04-thuc-thi/phan-cong/QUYET-DINH-THUC-THI-SOLO.md` and `README.md`.
3. Confirm the Task ID exists with owner `WSA-7K2` in `../../../docs/03-ke-hoach-giai-doan-a/BANG-IMPORT-TONG-GIAI-DOAN-A.md`.
4. Read `../../../docs/04-thuc-thi/phan-cong/WSA-7K2/DANH-SACH-TASK.md`.
5. Read the target module overview, decisions, backlog, analysis and assessment when relevant.
6. Read only the target slice, REL/CT, gate and direct dependency documents.

Do not begin from an isolated task row. Reconstruct module, dependency, gate and decision context first.

## Use the one-person model

- `WSA-7K2` owns all 167 tasks / 412 points in Giai đoạn A: M01, M02, M11, M12 and coordination.
- `WSA-9M4` is archived and must not receive new tasks, status updates or handoffs.
- Cross-module dependencies stay explicit but do not require handoff records.
- `WSA-7K2` makes all working decisions, performs the checks, and accepts completion. No separate reviewer, authority, approver, or signature is required.

## Work in the current checkout

Before editing, read `../../../docs/04-thuc-thi/phan-cong/QUY-TAC-NHANH-LAM-VIEC.md` and the WSA-7K2 startup prompt.

- Use the current `vocamon-project` checkout. Do not create or require a dedicated worktree or alias branch.
- Inspect existing changes before editing; preserve unrelated user work.
- Do not merge, rebase, push, delete branches or rewrite history without explicit authorization.

## Update sources in order

When a decision or scope changes:

1. Decision/analysis source.
2. Module backlog and Definition of Done.
3. Stage/slice plan and master import table.
4. Unified task list and WSA-7K2 work log.
5. REL/CT, evidence register and gate only when directly affected.

When only status changes, update the unified task list and work log first, then a shared tracker when one exists. Do not maintain duplicate personal trackers.

## Apply status rules

Use only: `Chưa bắt đầu`, `Đang thực hiện`, `Bị chặn`, `Hoàn thành`.

- Move to `Đang thực hiện` when work actually starts.
- Move to `Hoàn thành` when the output exists and the executor's selected checks pass.
- Keep a sliced task's parent open while deferred scope remains.
- For `Bị chặn`, record the missing data, tool, or real technical dependency and the next action.
- Waiting for another person's approval is never a blocker.

## Keep records minimal

- Use one task list and one work log.
- Group related work in batches, but record each task whose status changes.
- Evidence IDs are optional and used only when durable traceability is useful.
- Do not create new BG handoffs; represent cross-module needs as task dependencies or blockers.
- Never store secrets, tokens, personal data or raw payloads.

## Validate before finishing

- Confirm the master import and unified list each contain exactly 167 unique Task IDs / 412 points.
- Confirm every master row has exactly one owner, `WSA-7K2`.
- Confirm task IDs, dependencies, baseline, parent and gates remain synchronized.
- Check changed Markdown links and tables.
- Confirm no source-code file changed during a documentation-only request.
- Summarize status transitions, checks performed, unresolved blockers and next safe action.

---
name: wordsoul-doc-workflow
description: Execute and track WordSoul tasks with the simplified one-person documentation workflow. Use for task status, project decisions, optional evidence, REL/CT, gates, or files under WordSoulApi/docs. Do not use for source-code implementation unless separately requested.
---

# WordSoul workflow — one person

## Read only what is needed

1. Read `../../../docs/PROJECT.md` for boundaries and Definition of Done.
2. Read `../../../docs/DECISIONS.md` for active decisions.
3. Find the Task ID in `../../../docs/TASKS.md` and inspect its dependency and completion condition.
4. Read detailed module, REL/CT or gate documents only when the selected task requires them.

The numbered documentation folders are references. Do not update old import tables, personal trackers, handoff registers, slice trackers or work logs.

## Execute one task or one dependency-linked batch

1. Preserve unrelated changes in the current `vocamon-project` checkout.
2. Change the selected row in `TASKS.md` to `Đang thực hiện` when work starts.
3. Perform only the requested documentation or implementation scope.
4. Run checks appropriate to the changed boundary.
5. Put the short check result in `Kết quả`; Git supplies the commit history.
6. Mark `Hoàn thành` when the output exists and checks pass; otherwise use `Bị chặn` with the real missing input and next action.

WSA-7K2 makes decisions, performs checks and accepts completion. No reviewer, authority, signature, handoff or mandatory Evidence ID is required.

## Keep the record small

- `TASKS.md` is the only status tracker; Git is the work log.
- `DECISIONS.md` changes only for product, architecture, data, security, workflow or release decisions.
- `PROJECT.md` changes only when project boundaries, architecture, standards or common verification change.
- Use only `Chưa bắt đầu`, `Đang thực hiện`, `Bị chặn`, `Hoàn thành`.
- Keep a `-A` parent open while deferred scope remains.
- Evidence IDs are optional; never store secrets, tokens, personal data or raw payloads.

## Validate before finishing

- Confirm the selected Task ID remains unique and its dependency/completion condition is intact.
- Check changed links, tables and relevant build/test/lint results.
- Confirm the diff contains no unintended files or sensitive data.
- Update only the task row and any genuinely affected project/decision documentation, then commit the coherent batch.

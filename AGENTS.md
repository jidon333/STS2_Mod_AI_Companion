# Project Guardrails

## Commit Discipline
- Commit by work unit.
- Do not bundle unrelated code, docs, and deployment changes into one commit.

## Deployment Guardrails
- Stop the game before every deploy.
- Do not deploy while `SlayTheSpire2`, `crashpad_handler`, or related game processes are still running.
- If the game was running during deploy, treat the deploy as failed until the files are redeployed after shutdown.

## Mods Folder Hygiene
- Clean or explicitly reconcile the target mod outputs before deploy.
- Do not assume deleted or renamed assemblies disappeared from the game `mods` folder.
- If old DLLs, stale configs, or abandoned companion files can remain, remove them or verify the exact deployed file set before launching the game.

## Validation Guardrails
- Treat stale deploy, partial copy, and ABI mismatch states as invalid test conditions.
- If runtime behavior does not match the current source, verify deployed DLL timestamps and dependency identity before debugging gameplay behavior.
- Do not trust black screen, crash, harness, or scene-flow results until deployment identity is confirmed.

## Encoding Guardrails
- Markdown in this repository must be saved as UTF-8 with BOM.
- Reason: Windows PowerShell 5.1 misreads BOM-less UTF-8 Markdown as ANSI when `Get-Content` is used without an explicit `-Encoding`, which produced repeated Korean text corruption during doc updates.
- If a Markdown file renders as mojibake in the terminal, verify the file bytes before editing. Do not assume the file contents are already corrupted.
- When reading or writing Korean docs from the shell, prefer explicit UTF-8 operations such as `Get-Content -Encoding UTF8` and UTF-8 writes.
- `.editorconfig` is the repository-level source of truth for Markdown encoding. Do not save `.md` files in ANSI, UTF-16, or BOM-less UTF-8.

## Harness Guardrails
- `Manual Clean Boot` is the first gate.
- Always launch the game for this gate with `cmd /c start "" "steam://rungameid/2868840"`.
- Before trusting any harness result, verify:
  - no external arm/session token is present
  - stale `actions.ndjson` does not auto-execute
  - test mode does not auto-enable
  - the game does not auto-progress from main menu

## Harness Observer Strategy
- For harness observer or scene-authority work, start from `artifacts/knowledge/decompiled` before tuning polling heuristics.
- Treat the runtime observer as an `event + polling mixed observer`.
- Use transition-oriented hooks for scene transition, screen-ready, and lifecycle-boundary judgments when decompiled flow supports them.
- Keep polling for continuous state, reconciliation, drift detection, and watchdog duties.
- Do not treat a transient polled scene from `state.latest.json` as `scene-ready` by itself.
- Do not blanket-ban `_Ready`, `_EnterTree`, `Open`, `Setup`, `ShowScreen`, or `Refresh`; evaluate them as narrow scene-specific candidates first.
- Do not reopen `dispatch_node` while scene authority still depends on transient published snapshots.
## Preferred Pre-Deploy Checklist
1. Stop the game and confirm related processes are gone.
2. Rebuild the intended artifacts.
3. Clean or reconcile the target `mods` payload.
4. Deploy all dependent DLLs together.
5. Verify deployed timestamps or identities.
6. Only then run the next validation cycle.

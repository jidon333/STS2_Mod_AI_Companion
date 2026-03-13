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

## Harness Guardrails
- `Manual Clean Boot` is the first gate.
- Always launch the game for this gate with `cmd /c start "" "steam://rungameid/2868840"`.
- Before trusting any harness result, verify:
  - no external arm/session token is present
  - stale `actions.ndjson` does not auto-execute
  - test mode does not auto-enable
  - the game does not auto-progress from main menu

## Preferred Pre-Deploy Checklist
1. Stop the game and confirm related processes are gone.
2. Rebuild the intended artifacts.
3. Clean or reconcile the target `mods` payload.
4. Deploy all dependent DLLs together.
5. Verify deployed timestamps or identities.
6. Only then run the next validation cycle.

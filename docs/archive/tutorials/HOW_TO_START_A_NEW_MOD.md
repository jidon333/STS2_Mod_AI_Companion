# Local Setup Notes

> Status: Archive
> Source of truth: No
> This is an older local bootstrap note kept for history only. Prefer [tutorials/MODDING_FROM_ZERO.md](../../tutorials/MODDING_FROM_ZERO.md).

This repository is already materialized from the shared scaffold. Use this file as a local bootstrap checklist for the AI Companion mod.

Recommended order:

1. Update `config\ai-companion.sample.json` with your local game paths and author metadata.
2. Keep `src\Sts2ModAiCompanion.Mod` minimal until the bridge design is settled.
3. Build and run the self-test before touching packaging or deployment flows.
4. Use `Sts2ModKit.Tool` for snapshot, restore, package staging, and deployment dry runs.
5. Leave AI logic, gameplay automation, and multiplayer behavior for later phases described in `docs\ROADMAP.md`.

Current naming baseline:

- display name: `STS2 Mod AI Companion`
- runtime package: `sts2-mod-ai-companion.pck`
- managed payload: `sts2-mod-ai-companion.dll`
- runtime config: `sts2-mod-ai-companion.config.json`
- package folder: `Sts2ModAiCompanion`

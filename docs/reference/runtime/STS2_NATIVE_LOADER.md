# STS2 Native Mod Loader

> Status: Reference
> Source of truth: No
> Use this for loader background, not for current blocker or current-state 판단.

The native loader looks for content in the game's `mods` directory.

Expected package shape for this repo:

- `sts2-mod-ai-companion.pck`
- `sts2-mod-ai-companion.dll`
- `Sts2ModKit.Core.dll`
- `sts2-mod-ai-companion.config.json`

Loader assumptions:

1. The `.dll` basename must match the `.pck` basename.
2. `mod_manifest.json` is embedded inside the `.pck`, not shipped loose.
3. `mod_manifest.json` must describe the same `pck_name` basename as the packaged file.
4. Harmony bootstrap stays minimal until the project moves beyond the current scaffold phase.

Operational note:

- Treat native packaging as a thin loader shell around the future external companion design, not as the place where AI logic will live.

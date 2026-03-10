# STS2 Mod AI Companion

Initial Slay the Spire 2 mod repository for the AI Companion project.

Current scope:

- clean native mod skeleton based on the local scaffold
- packaging, snapshot, restore, and self-test tooling
- project documentation for roadmap, architecture, and boundaries

Deferred on purpose:

- real AI feature implementation
- gameplay automation
- multiplayer intervention
- in-game UI or overlay work

Quick start:

1. Review `config\ai-companion.sample.json` and update local paths if needed.
2. Build the solution with `dotnet build STS2_Mod_AI_Companion.sln`.
3. Run `dotnet run --project src\Sts2ModKit.SelfTest`.
4. Use `dotnet run --project src\Sts2ModKit.Tool -- show-config` to inspect the resolved configuration.

Key docs:

- `docs\ROADMAP.md`
- `docs\ARCHITECTURE.md`
- `docs\BOUNDARIES.md`
- `docs\STS2_NATIVE_LOADER.md`
- `docs\BACKUP_AND_ROLLBACK.md`
- `docs\SMOKE_TEST_CHECKLIST.md`
- `docs\HOW_TO_START_A_NEW_MOD.md`

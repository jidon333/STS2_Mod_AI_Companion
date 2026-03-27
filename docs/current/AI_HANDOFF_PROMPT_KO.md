# 새 세션용 AI 인수인계 프롬프트 (KO)

아래 프롬프트를 그대로 새 구현 세션에 넣으면 됩니다.

```text
사령관님 지시입니다.

이번 세션은 오염된 이전 세션을 이어받지 말고, current `main` 기준으로 새로 시작합니다.

repo root:
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion

반드시 먼저 읽을 것

1. guardrails
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/AGENTS.md

2. current status / handoff
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/AI_SESSION_HANDOFF_KO.md

3. harness architecture / module boundaries
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md

4. current checklist / matrix
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/HARNESS_REGRESSION_CHECKLIST_KO.md
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/HARNESS_FIXTURE_MATRIX_KO.md

현재 확정 상태

- current `main`의 smoke harness refactor wave 1-8은 완료됨
- `Program.cs`는 shell-only임
- build / `Sts2ModKit.SelfTest` / `Sts2GuiSmokeHarness -- self-test` / `replay-test`는 green
- `replay-parity-test`는 single known red `reward-aftermath-map-handoff` baseline을 유지 중
- `WaitRunLoad` resumed reward/map mixed handoff bug는 이미 닫힘
- latest valid live first blocker는 reward aftermath 이후 `ChooseFirstNode` `decision-wait-plateau`

이번 세션 목표

1. old `Program.cs` monolith 전제를 버리고 current owner 파일 기준으로 시작
2. 구조 refactor reopen 금지
3. semantic fix가 필요하면 reward aftermath 이후 `ChooseFirstNode / WaitMap / map-node` gap만 좁게 수정
4. new parallel heuristic family 추가 금지
5. acceptance 통과 시 바로 커밋

우선 열 파일

- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.PhaseLoopRouting.cs
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.NonCombat.cs
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Observer/GuiSmokeObserverPhaseHeuristics.cs
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatSceneState.cs
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatDecisions.cs

핵심 원칙

- `currentScreen`은 logical/flow screen이다
- owner 용어는 `canonical foreground owner`
- `visible/open != owner`
- green 구조 refactor reopen 금지
- 구조 변경과 semantic fix를 같은 commit에 섞지 말 것

canonical current signal

- latest valid live root:
  - /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/longrun-artifacts-split-20260327-live1/startup-summary.json
  - /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/longrun-artifacts-split-20260327-live1/attempts/0001/failure-summary.json
  - /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/longrun-artifacts-split-20260327-live1/attempts/0001/run.log

시작 보고 요구

- 구현 전 아래 4개만 짧게 보고하고 시작할 것
  1. current semantic gap 재진술
  2. direct owner files
  3. 최소 수정 표면
  4. acceptance 기준

검증

- `cmd.exe /c dotnet build STS2_Mod_AI_Companion.sln`
- `cmd.exe /c dotnet run --project src/Sts2ModKit.SelfTest/Sts2ModKit.SelfTest.csproj --no-build`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- self-test`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-test`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-parity-test`
- semantic acceptance에 live root가 필요하면 fresh run 1회:
  - `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj -- run --scenario boot-to-long-run --provider auto --run-root artifacts/gui-smoke/<fresh-root> --max-attempts 1 --max-steps 60 --stop-on-first-terminal --stop-on-first-loop`

acceptance

- 구조 refactor를 reopen하지 않음
- touched owner files만 수정함
- build/self-test/replay-test green 유지
- replay parity는 `reward-aftermath-map-handoff` single known red만 유지
- fresh live를 돌렸다면 새 first authoritative blocker 1개만 보고
- acceptance 통과 시 묻지 말고 즉시 커밋
```

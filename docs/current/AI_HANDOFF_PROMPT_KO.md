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

3. harness architecture / cleanup baseline
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md

4. current checklist / matrix
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/HARNESS_REGRESSION_CHECKLIST_KO.md
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/HARNESS_FIXTURE_MATRIX_KO.md

현재 확정 상태

- current `main`의 smoke harness refactor wave 1-8은 완료됨
- `Program.cs`는 shell-only임
- build / `Sts2ModKit.SelfTest` / `replay-test` / `replay-parity-test`는 green
- `Sts2GuiSmokeHarness -- self-test`는 현재 unrelated known red 1건이 있음
- `WaitRunLoad` resumed reward/map mixed handoff bug는 이미 닫힘
- published-first observer provenance migration은 current `main`에 반영됨
- combat stale-end-turn / target plateau family는 current `main`에서 micro-stage + quiet convergence + runtime target-summary authority로 닫힘
- latest cleanup proof root는 [20260329-162955-boot-to-long-run](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run)이고 `WaitMainMenu -> EnterRun`, `WaitRunLoad -> HandleCombat`, combat post-wait recapture continuity가 모두 fresh live로 확인됨
- latest combat blocker fix root는 [combat-target-summary-20260330-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2)이고 stale non-enemy end-turn / target plateau가 재현되지 않음
- latest speed proof root는 [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9)이고 explicit event `step=8~9`, common combat `step=11~17`이 모두 `captureMode=skipped`, `sceneReasoningMode=observer-only`였음
- `COMBAT-03`, `COMBAT-07`, `REWARD-04` coverage row는 current `main`에서 green
- `MAP-04`는 representative live continuity는 있으나 current harness self-test red 때문에 `partial`

이번 세션 목표

1. old `Program.cs` monolith 전제를 버리고 current owner 파일 기준으로 시작
2. cleanup-complete baseline reopen 금지
3. semantic follow-up 우선순위는 unrelated `WaitPostMapNodeRoom -> reward reopen` self-test red, 그 다음 low-priority coverage frontier (`EVENT-05`, `REWARD-10`)
4. noncombat mixed-state guard cleanup은 reopen 금지
5. new parallel heuristic family 추가 금지
6. acceptance 통과 시 바로 커밋

우선 열 파일

- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatSceneState.cs
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.PhaseLoopRouting.cs
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.SelfTests.PhaseRouting.EnterRunAndPostNode.cs
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Observer/GuiSmokeObserverPhaseHeuristics.cs
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.SelfTests.EventRewardSubstates.cs
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.SelfTests.StallSentinel.cs

핵심 원칙

- `currentScreen`은 logical/flow screen이다
- owner 용어는 `canonical foreground owner`
- `visible/open != owner`
- green 구조 refactor reopen 금지
- 구조 변경과 semantic fix를 같은 commit에 섞지 말 것

canonical current signal

- latest cleanup proof root:
  - /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/startup-summary.json
  - /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/session-summary.json
  - /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/attempts/0001/run.log
- latest speed proof root:
  - /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/startup-summary.json
  - /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/attempts/0001/run.log
  - /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/attempts/0001/failure-summary.json

시작 보고 요구

- 구현 전 아래 4개만 짧게 보고하고 시작할 것
  1. current semantic gap 재진술
  2. direct owner files
  3. 최소 수정 표면
  4. acceptance 기준

검증

- `cmd.exe /c dotnet build STS2_Mod_AI_Companion.sln`
- `cmd.exe /c dotnet run --project src/Sts2ModKit.SelfTest/Sts2ModKit.SelfTest.csproj --no-build`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-test`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-parity-test`
- known current red:
  - `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- self-test`
  - `WaitPostMapNodeRoom should reopen reward handling when the destination room is rewards.`
- semantic acceptance에 live root가 필요하면 fresh run 1회:
  - `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj -- run --scenario boot-to-long-run --provider auto --run-root artifacts/gui-smoke/<fresh-root> --max-attempts 1 --max-steps 60 --disable-video-capture`

acceptance

- 구조 refactor를 reopen하지 않음
- touched owner files만 수정함
- build/self-test/replay-test green 유지
- replay parity는 green 유지
- fresh live를 돌렸다면 새 first authoritative blocker 1개만 보고
- acceptance 통과 시 묻지 말고 즉시 커밋
```

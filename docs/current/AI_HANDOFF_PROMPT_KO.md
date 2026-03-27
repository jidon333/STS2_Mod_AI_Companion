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

2. handoff
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/AI_SESSION_HANDOFF_KO.md

3. current checklist / matrix
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/HARNESS_REGRESSION_CHECKLIST_KO.md
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/HARNESS_FIXTURE_MATRIX_KO.md

4. canonical blocker root
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/stage4-shared-noncombat-20260327-live1/startup-summary.json
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/stage4-shared-noncombat-20260327-live1/attempts/0001/failure-summary.json
- /mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/stage4-shared-noncombat-20260327-live1/attempts/0001/run.log

현재 확정 상태

- current `main`에는 아래 리팩터링이 이미 들어가 있음
  - `ce038d3` reward canonicalization
  - `33f5edf` combat post-wait recapture boundary
  - `bc84751` event/map release handoff
  - `4e0cd4a` shared noncombat owner/release contracts
- build / `Sts2ModKit.SelfTest` / `Sts2GuiSmokeHarness -- self-test` 는 green
- current first authoritative blocker는 Stage 4 regression이 아니라
  - `WaitRunLoad`가 resumed reward/map mixed state를 room phase로 handoff하지 못하는 문제

blocker direct cause

- live root는 valid다
- 하지만 step 8~37 동안 observer가 계속:
  - `currentScreen=rewards`
  - `visibleScreen=map`
  - `sceneReady=false`
  - `sceneStability=stabilizing`
  - `choiceExtractorPath=reward`
  - explicit reward choices visible
- 그런데 `TryGetPostRunLoadPhase(...)`가 `sceneReady=false` gate 때문에 room branch를 열지 못한다
- 그 결과 `WaitRunLoad` timeout으로 죽는다

핵심 원칙

- `currentScreen`은 logical/flow screen이다. visual top-layer로 읽지 말 것
- owner 용어는 `canonical foreground owner`
- `visible/open != owner`
- timer/count heuristic으로 덮지 말 것
- green 상태 reopen 금지
- 기존 canonical state / shared contract 밖에 새 parallel heuristic 추가 금지

이번 세션 목표

1. `WaitRunLoad` handoff bug만 좁게 수정
2. `sceneReady=false`이더라도 explicit room authority가 있으면 적절한 room phase로 탈출시키기
3. reward/event/shared contract 자체는 되도록 건드리지 않기
4. build/self-test/live root 1회로 first blocker closure 확인
5. acceptance 통과 시 바로 커밋

주요 코드 타깃

- `/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs`

특히 다시 볼 것

- `GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(...)`
- `GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(...)`
- `RootSceneTransitionObserverSignals.ShouldHoldRunLoadBoundary(...)`
- `NonCombatForegroundOwnership.Resolve(...)`
- `TryBuildCanonicalNonCombatSceneState(...)`

시작 보고 요구

- 구현 전 아래 4개만 짧게 보고하고 시작할 것
  1. live1 direct cause 재진술
  2. 왜 이게 Stage 4 regression이 아닌지
  3. 최소 수정 표면
  4. acceptance 기준

수정 계약

- `sceneReady=false`만으로 무조건 막지 말 것
- 단, 실제 transition/loading 보호는 유지할 것
- explicit room authority가 있는 resumed reward/event/shop/rest/treasure/combat state라면 room phase handoff 허용
- invalid root와 gameplay blocker를 섞지 말 것
- broad rewrite 금지

검증

- `cmd.exe /c dotnet build STS2_Mod_AI_Companion.sln`
- `cmd.exe /c dotnet run --project src/Sts2ModKit.SelfTest/Sts2ModKit.SelfTest.csproj --no-build`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- self-test`
- fresh live run 1회:
  - `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj -- run --scenario boot-to-long-run --provider auto --run-root artifacts/gui-smoke/<fresh-root> --max-attempts 1 --max-steps 60 --stop-on-first-terminal --stop-on-first-loop`

acceptance

- live1 family가 더 이상 `WaitRunLoad`에서 reward/map mixed state로 plateau하지 않음
- resumed reward authority면 `HandleRewards` 등 올바른 room phase로 handoff됨
- build/self-tests green 유지
- 새 first authoritative blocker가 나오면 그 1개만 보고
- acceptance 통과 시 묻지 말고 즉시 커밋

커밋 제목

- `Allow WaitRunLoad to hand off resumed room authority`

최종 보고 형식

1. direct cause
2. exact gate fixed
3. actual code changes
4. build/self-test/live 결과
5. fresh live root path
6. root validity
7. blocker closure 여부
8. new first authoritative blocker 여부
9. commit hash
10. remaining dirty inventory
```

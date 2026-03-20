# 상세 조사 로그

> Status: Archive
> Source of truth: No
> This is historical investigation history. Do not use it as the current blocker or current-state source.

이 문서는 증상, 시도, 실패 원인, 최종 판단을 조금 더 자세히 남기는 조사 로그입니다.

## 1. native route 재확인

### 증상

- 초기에 저장소가 scaffold 수준이라 실제 STS2 live state를 안정적으로 꺼낼 수 있는 경로가 부족했습니다.

### 시도

- scaffold와 기존 speed mod 저장소를 읽어 native route, packaging, snapshot/restore 습관을 비교했습니다.

### 판단

- 현재 저장소는 `passive save scraping`보다 `read-only Harmony exporter`를 중심으로 가는 것이 맞다고 판단했습니다.

## 2. Harmony `PatchAll` startup failure

### 증상

- `godot.log`에 startup 시점 예외가 발생했고 live export가 bootstrap 이후 진행되지 않았습니다.

### 원인

- broad hook 후보와 void 메서드 postfix 처리 경로가 섞이면서 Harmony가 안전하게 패치하지 못했습니다.

### 조치

- hook registry를 더 좁게 잡았습니다.
- startup identity와 hook summary를 런타임 로그에 남기도록 정리했습니다.
- polling fallback을 유지해 main menu baseline을 복구했습니다.

## 3. smoke 절차의 맹점

### 증상

- `inspect-godot-log --lines 200`가 startup failure를 놓칠 수 있었습니다.

### 원인

- tail 위주 확인 절차는 로그 초반 예외를 놓칠 수 있습니다.

### 조치

- inspect 결과에 startup findings를 따로 노출하는 방향으로 진단 레이어를 보강했습니다.

## 4. 정적 지식 추출이 비어 있던 문제

### 초기 상태

- live observation만으로는 카드/유물/이벤트/상점 본문 지식이 거의 채워지지 않았습니다.

### 판단

- 관찰 기반 카탈로그가 충분히 채워질 때까지 기다리지 말고 오프라인 스캔을 공격적으로 같이 진행해야 했습니다.

### 조치

- `assembly-scan`
- `pck-inventory`
- `observed-merge`

를 묶어 `extract-static-knowledge`에서 함께 생성하도록 변경했습니다.

## 5. inspect-static-knowledge의 이상한 출력

### 증상

- 한 번은 extract 직후 inspect가 intermediate artifact를 못 찾고 0건처럼 보였습니다.

### 원인

- extract와 inspect를 병렬로 돌려 inspect가 먼저 읽었습니다.

### 판단

- 코드 버그가 아니라 실행 순서 문제였습니다.

### 현재 처리

- static knowledge 검증은 extract 이후 inspect를 순차 실행하는 것으로 다시 확인했습니다.

## 6. WPF / host 도입 시의 최소 원칙

### 판단

- 모드가 외부 프로세스를 spawn하는 구조는 넣지 않습니다.
- WPF 앱이 먼저 켜져 있거나 나중에 켜져도 live export에 attach하는 구조가 더 안전합니다.

### 이유

- 실패 시 게임과 외부 앱을 분리해서 볼 수 있습니다.
- Phase 1 경계와도 잘 맞습니다.

## 7. 남은 조사 포인트

- reward/event/shop/rest/combat에서 semantic hook가 실제로 어느 타입/메서드에 잘 걸리는가
- PCK inventory 결과를 실제 카드/유물/이벤트 canonical id로 어떻게 정규화할 것인가
- replay harness를 어떤 fixture 세트로 구성할 것인가

## 8. harness contamination이 결과 전체를 오염시키는 문제

### 증상

- 사용자가 새 명령을 내리지 않았는데도 main menu에서 character select / run start 쪽으로 자동 진행되는 것처럼 보이는 런이 있었습니다.
- stale harness inbox에 `__start__`, `__ironclad__`, `__confirm__` 같은 이전 액션이 남아 있었습니다.
- bridge load 시점에 test mode / FTUE bypass가 곧바로 활성화되는 경로가 존재했습니다.

### 판단

- 이 상태에서는 black screen, crash, scene contamination, harness PoC 결과를 정상 기준으로 해석하면 안 됩니다.
- 우선순위는 `first reward`가 아니라 `Manual Clean Boot`입니다.
- stale deploy/ABI mismatch가 해결되어도 harness contamination이 남아 있으면 런 전체를 신뢰할 수 없습니다.

### 조치

- bridge 기본 상태를 `dormant`로 재정의했습니다.
- `arm.json`이 없으면 queue를 소비하지 않도록 경계를 재설계했습니다.
- session token이 일치하는 action만 소비하도록 방어선을 추가했습니다.
- bridge load 시 test mode auto-enable은 허용하지 않고, arm 이후에만 가능하도록 바꾸는 방향으로 고정했습니다.

## 9. external command transport를 왜 파일 기반으로 먼저 고정했는가

### 고민

- observer는 파일 기반 live export이고, command만 IPC로 올리는 혼합형이 직관적으로 좋아 보일 수 있었습니다.

### 판단

- 현재 iteration의 핵심은 지연시간보다 contamination 차단과 triage 가능성입니다.
- 이미 `live export`, `harness inbox/outbox`, `status.json`, `trace.ndjson`가 파일 기반이므로, 같은 경계에 `arm.json`과 `inventory.latest.json`을 추가하는 편이 더 안전합니다.
- transport를 섞으면 session sync, boot race, stale command 판정 지점이 늘어납니다.

### 결론

- 이번 단계는 파일 기반 external command contract로 고정합니다.
- latency가 실제 병목으로 확인되기 전까지는 IPC로 올리지 않습니다.

## 10. 멀티 에이전트 Codex orchestration 자체의 실패

### 증상

- 서브 에이전트가 repository 파일을 읽거나 쓰려는 시점에 backend가 `refusing to run unsandboxed` 류 오류를 내며 막혔습니다.
- `fork_context=true`로 다시 붙여도 critical path worker로 신뢰할 수 있는 수준까지는 복구되지 않았습니다.

### 판단

- 이 세션에서는 멀티 에이전트를 주 작업 경로로 쓰면 안 됩니다.
- 특히 구현/빌드/검증을 worker에 의존하는 전략은 지금 상태에서 실무적으로 불안정합니다.

### 조치

- 이번 iteration 동안은 메인 세션 단일 실행 주체 전략으로 전환했습니다.
- 서브 에이전트는 critical path에서 제외하고, 문서/설계 참고용 보조 수단으로만 취급하기로 했습니다.
- 이 판단 자체도 시행착오로 기록해 다음 세션이 같은 시간 손실을 반복하지 않도록 합니다.

## 11. WSL -> Windows fresh smoke run에서 드러난 실제 병목

### 실행 조건

- 실행 주체는 WSL 세션이었지만, 실제 빌드/런은 Windows `dotnet.exe`를 사용했습니다.
- WSL 셸에는 `dotnet`이 없었고, `/mnt/c/Program Files/dotnet/dotnet.exe` 경로를 직접 써야 했습니다.
- fresh run artifact root는 `artifacts/gui-smoke/auto-fresh-run-wsl-20260314-213832` 입니다.

### 관찰된 사실

- `main-menu -> continue -> map` 진입은 정상 진행되었습니다.
- treasure room에서 첫 `center click`은 실제로 chest를 여는 데 성공했습니다.
- 근거:
  - `0004.screen.png`는 닫힌 chest 상태입니다.
  - `0006.screen.png`, `0011.screen.png`는 열린 chest와 중앙 상단의 떠 있는 보상 아이콘이 보입니다.
- 즉 `treasure chest = center click` 자체는 1차 동작했습니다.

### 실제 실패 원인

- harness가 열린 chest 상태를 별도 phase/substate로 구분하지 못했습니다.
- observer는 계속 `encounter=Treasure`, `choices=[Chest, 진행, ...]` 형태로 남아 있었고, harness는 이를 근거로 다시 `treasure chest center`를 반복했습니다.
- 하지만 스크린샷 기준 실제 actionable target은 닫힌 chest 본체가 아니라, 열린 chest 위에 떠 있는 보상 아이콘 쪽이었습니다.

### 추가 증거

- `steps/0011.request.json` 시점에도 스크린샷은 열린 chest인데 decision은 여전히 `treasure chest center` 였습니다.
- `lastEventsTail`에는 일시적으로 `CardGrid`가 보였다가 다시 사라지는 흔적이 있어, observer choice set만으로는 안정적으로 이 substate를 잡기 어렵습니다.
- 따라서 이 구간은 계속 screenshot-first substate 인식이 우선입니다.

### 운영 중 추가 트러블

- 사용자가 중간에 프로세스를 중단하자 window handle이 사라졌고, harness는 `virtual-screen-fallback`으로 떨어지며 `capture unusable`을 반복했습니다.
- 이는 fresh run 자체의 논리 실패와 별개로, Windows 쪽 프로세스 중단 시 현재 attach session은 바로 버리는 편이 맞다는 운영 팁을 다시 확인해 줍니다.

### 현재 판단

- 다음 수정 포인트는 `treasure closed chest`와 `treasure opened reward icon`을 스크린샷 기준으로 분리하는 것입니다.
- 이 구간은 observer stale state를 더 정교하게 읽는 것으로 해결할 문제가 아니라, screenshot-first reward pickup 판단을 추가해야 닫힙니다.

## 12. WSL live run 2차에서 확인된 실제 운영 이슈와 phase 병목

### 포커스가 게임으로 계속 튀는 이유

- 원인은 harness 쪽 입력/캡처 진입 코드였습니다.
- `WindowLocator.EnsureInteractive(...)`가 캡처와 액션 직전에 `ShowWindow`, `BringWindowToTop`, `SetForegroundWindow`를 호출합니다.
- `MouseInputDriver.Click(...)`, `RightClick(...)`, `PressKey(...)`도 입력 직전에 다시 `SetForegroundWindow`를 호출합니다.
- 따라서 live run 중에는 CLI 포커스를 안정적으로 유지할 수 없습니다.
- 이 현상은 게임 쪽 버그가 아니라 현재 harness의 foreground 강제 정책에서 온다는 점을 기록합니다.

### treasure room의 실제 substate

- `auto-fresh-run-wsl-20260314-214707`과 `auto-fresh-run-wsl-20260314-215110`에서 treasure 흐름을 다시 확인했습니다.
- 규칙은 다음처럼 정리됐습니다.
  - 닫힌 상자면 먼저 chest center를 클릭합니다.
  - 상자가 열려 있고 떠 있는 유물/보상 아이콘이 보이면 그 아이콘을 먼저 클릭합니다.
  - 빈 상자일 때만 `Proceed`를 우선합니다.
- 이 단계는 observer `choices`보다 스크린샷 substate가 더 중요합니다.

### rest site smith confirm의 실제 의미

- 사용자 힌트로 `V`는 키보드 `V`가 아니라 화면 오른쪽 중간의 확인 버튼이라는 점을 확정했습니다.
- `auto-fresh-run-wsl-20260314-220249`에서
  - `rest site: smith`
  - `rest site: smith card`
  - `rest site: smith confirm`
  순서가 실제로 수행됐고,
- 이후 `visible proceed -> visible reachable node -> WaitCombat accepted`까지 이어져 다시 전투에 진입했습니다.
- confirm 좌표는 실스크린샷을 보고 오른쪽 중간 버튼 쪽으로 보정했습니다.

### 전투 재진입 이후 새 병목

- `auto-fresh-run-wsl-20260314-220527`에서는 smith 이후 elite 전투 재진입까지 성공했습니다.
- 하지만 step 15부터 decision이 계속 `right-click cancel unresolved selected card`로 고정됐습니다.
- 확인 결과 원인은 observer가 아니라 combat screenshot analyzer 쪽입니다.
  - `AutoCombatAnalyzer.HasSelectedCard(...)`는 손패가 보이는 일반 전투 시작 화면도 selected-card로 오탐할 수 있습니다.
  - 실제 `0015.screen.png`에서는 아직 harness가 어떤 카드도 고르지 않았는데, `DecideHandleCombat(...)`가 곧바로 cancel 분기로 들어갔습니다.
- 즉 병목은 `selected-card resolution` 분기가 너무 일찍 열리는 데 있습니다.

### 운영 중 추가 트러블

- live run 중 빌드 산출물 `Sts2GuiSmokeHarness.exe`가 살아 있어 output lock으로 빌드가 한 번 막혔습니다.
- 다음 deploy/build 전에는 harness 프로세스까지 같이 내려야 합니다.
- 이번 세션에서는 `SlayTheSpire2`, `crashpad_handler`, `Sts2GuiSmokeHarness`를 모두 정리한 뒤에만 다음 수정/검증을 진행했습니다.
- 세션 후반에는 WSL에서 `cmd.exe`, `powershell.exe`, `dotnet.exe` 호출이 모두 `UtilBindVsockAnyPort: socket failed 1`로 실패했습니다.
- 즉 이후 build/self-test/live-run 검증이 막힌 원인은 harness 코드가 아니라 WSL -> Windows interop 환경 자체였습니다.

## 13. combat opener를 screenshot-first로 되돌린 뒤 실제로 드러난 다음 병목

### `right-click cancel` 루프는 실제로 끊어졌다

- `auto-fresh-run-wsl-20260314-221505`에서 전투 오프너를 다시 확인했습니다.
- step 4에서 decision이 더 이상 `right-click cancel unresolved selected card`로 가지 않았습니다.
- 실제 흐름은 다음처럼 진행됐습니다.
  - `combat select attack slot 1`
  - `auto-target enemy`
  - 다시 손패를 읽고 다음 공격 선택
- 즉 기존 병목이던 "전투 시작 직후 selected-card 오탐 -> 무한 cancel" 경로는 이번 수정으로 실제 run 기준 끊겼습니다.

### 새 병목은 손패 재배열 뒤 `slot index`를 정적으로 믿은 점이었다

- 같은 run의 `0006.screen.png`, `0008.screen.png`를 보면 공격 카드가 실제로 플레이되어 적 HP가 `91/91 -> 82/91 -> 64/91`까지 내려갑니다.
- 즉 harness는 이제 전투 행동 자체를 실제로 수행하고 있습니다.
- 하지만 손패가 줄어든 뒤 카드가 다시 가운데로 재배열되는데, 당시 로직은 고정된 5슬롯 샘플 박스를 그대로 사용했습니다.
- 그 결과 `0008.screen.png`처럼 남은 손패가 `[공격, 방어+, 방어]`가 된 상태에서도 analyzer가 `2번 슬롯`을 공격 카드로 오인했습니다.
- 실제 화면에서는 공격 카드가 다시 `1번`이었는데 harness는 계속 `combat select attack slot 2 -> auto-target enemy`를 반복했습니다.
- 이 병목은 observer stale state와 무관하며, pure screenshot phase routing 안에서 `현재 손패의 동적 재배열`을 읽지 못한 문제입니다.

### 여기서 선택한 단순화

- smoke harness 목적상 완전한 카드 인식보다 안정적인 black-box 진행이 더 중요하므로, 우선 로직을 더 단순하게 바꿨습니다.
- 남아 있는 공격 카드가 보이면 `현재 손패의 1번`부터 소비하는 쪽으로 되돌렸습니다.
- 공격이 더 없으면 마찬가지로 `현재 손패의 1번`을 non-attack/defend confirmation 흐름으로 태우도록 바꿨습니다.
- 즉 "정적 슬롯 번호를 맞히는 문제"보다 "현재 화면에서 사람이 하듯 왼쪽부터 소모하는 흐름"을 우선했습니다.

### 이후 fresh run의 launch/attach 플래키함

- 위 수정 뒤 `build`, `self-test`는 다시 통과했습니다.
- 이어서 `auto-fresh-run-wsl-20260314-221835`로 fresh run을 다시 붙였지만,
  - `runtime config ensured`
  - `waiting for STS2 window`
  - `window detected after launch`
  까지만 기록되고, 그 뒤 `step=1` 캡처 로그가 더 이상 생성되지 않았습니다.
- 이 run root에는 `run.json`, `run.log`만 있고 `steps/`가 생성되지 않았습니다.
- 당시 Windows 프로세스는 실제로 살아 있었으므로, 이 실패는 gameplay 병목이 아니라 `launch/attach -> first capture` 구간의 운영 플래키함으로 분리해서 봐야 합니다.
- 사용자 운영 팁대로 이런 세션은 이어서 디버깅하지 말고 바로 fresh run으로 버리는 쪽이 맞습니다.

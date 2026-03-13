# Harness Mode

Harness mode는 개발/QA 단계에서 사용하는 test-only 운영 모드입니다. production advisor와 foundation을 공유하지만, 입력 실행이 허용된다는 점이 다릅니다.

## 1. 목적

- 사람 없이도 최소 시나리오를 자동 반복 실행
- exporter / knowledge / advice 파이프라인 회귀 검증
- failure를 artifact로 남겨 재현과 triage를 쉽게 만들기

## 2. 구성 요소

### Harness Runner
- `src/Sts2AiCompanion.Harness/Scenarios/ScenarioRunner.cs`
- 시나리오를 읽고 상태-행동 루프를 실행합니다.

### Action Executor
- `BridgeActionExecutor`
- `UiAutomationActionExecutor`

기본 선택은 bridge입니다. UI automation은 bridge가 불가능한 경우의 보조 경로입니다.

### Recovery / Evaluation / Replay
- `RecoveryManager`
- `AcceptanceEvaluator`
- `ReplayController`

## 3. 행동 모델

현재 canonical action kind:

- `click_card`
- `click_button`
- `choose_reward`
- `choose_event_option`
- `choose_map_node`
- `end_turn`
- `confirm`
- `cancel`
- `restart_room`
- `relaunch_game`
- `emergency_escape`
- `noop`

## 4. 첫 PoC

첫 harness PoC는 `scenarios/smoke.ironclad.first-reward.json`입니다.

목표:
- main menu
- 아이언클래드 선택
- 첫 전투 진입
- 첫 보상 선택
- map 복귀

즉 production advisor를 개발하는 동안, 가장 짧은 자동 회귀 시나리오를 foundation 위에서 닫는 것이 목표입니다.

## 5. Production과의 경계

- production mod: read-only 유지
- harness bridge: test-only ingress
- production UI/WPF에는 action layer를 직접 넣지 않음
- harness artifact는 `artifacts/harness/` 아래에 분리

## Update (2026-03-13): dormant / arm / inventory / nodeId command

- harness bridge의 기본 상태는 `dormant`입니다.
- control contract는 아래 파일 경계로 고정합니다.
  - `arm.json`
  - `inventory.latest.json`
  - `actions.ndjson`
  - `results.ndjson`
  - `status.json`
  - `trace.ndjson`
- `arm.json`이 없으면 bridge는 queue를 소비하지 않습니다.
- external preferred command는 `dispatch_node`이며, command는 `nodeId + inventoryId + sessionToken`을 기준으로 검증됩니다.
- legacy semantic action은 하위 호환용으로 잠시 남길 수 있지만, 외부 AI control의 주 경로로는 더 이상 권장하지 않습니다.
- bridge는 node inventory를 export하고, 외부 세션이 어떤 `nodeId`를 실행할지 결정합니다.
- bridge가 허용하는 실행은 UI-bound event dispatch까지만입니다.

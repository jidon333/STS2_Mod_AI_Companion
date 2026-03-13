# STS2 Harness Review

## 1. 현재 구조 요약
- observer: `LiveExportStateTracker -> live export files -> CompanionStateMapper -> LiveCompanionStateSource` 경로가 여전히 기본 world model이다. 다만 이번 clean-boot 사이클에서는 harness bridge 쪽 `inventory.latest.json` 발행을 잠시 비활성화했다. 평가: `Weak`
- actuator: 이번 사이클의 bridge는 의도적으로 actuator를 열지 않는다. `arm.json`이 있어도 action은 실행하지 않고 `action-rejected`로 남긴다. 평가는 기능 부족이 아니라 clean-boot 기준선 복구를 위한 의도적 축소다. 평가: `Deferred`
- contract: 파일 기반 harness contract는 유지된다. 이번 사이클에서 실제로 닫힌 핵심은 `arm.json`, `actions.ndjson`, `status.json`, `trace.ndjson`이다. `inventory.latest.json`, `results.ndjson`는 post-clean-boot cycle로 보류했다. 평가: `OK`
- guards: dormant/armed 전환과 `arm.json` 기반 session guard는 유지된다. arm이 없으면 queue를 소비하지 않고 stale action은 `action-ignored`로만 남긴다. 평가: `OK`
- transport: `status.json`은 이제 live export와 같은 atomic replace로 쓴다. `trace.ndjson`는 shared append를 유지한다. 다만 전체 idempotency/ordering contract는 아직 약하다. 평가: `Improving`
- replay: replay 관련 artifact/fixture 경로는 남아 있지만 이번 clean-boot 사이클의 주력은 아니다. 평가: `Weak`

## 2. 잘된 점
- bridge가 기본적으로 dormant로 시작하고 `arm.json` 없이는 stale `actions.ndjson`를 실행하지 않도록 최소 경계를 다시 세웠다. 근거: `src/Sts2ModAiCompanion.HarnessBridge/HarnessBridgeHost.cs`
- `HarnessBridgeHost`의 책임을 최소 루프 기준으로 다시 분리했다. 현재는 `ArmSessionReader`, `ActionQueueScanner`, `StatusPublisher`, `TraceWriter`로 clean-boot triage에 필요한 책임만 남겼다. 근거: `src/Sts2ModAiCompanion.HarnessBridge/*.cs`
- `status.json` 기록을 atomic replace로 전환했다. 이전 review의 P0 중 하나를 이번 사이클에서 직접 반영했다. 근거: `src/Sts2ModAiCompanion.HarnessBridge/StatusPublisher.cs`, `src/Sts2ModKit.Core/LiveExport/LiveExportPaths.cs`
- stale action을 단순 무시가 아니라 `action-seen -> action-ignored` trace로 남겨 triage 가능성을 높였다. 근거: `src/Sts2ModAiCompanion.HarnessBridge/TraceWriter.cs`, `src/Sts2ModAiCompanion.HarnessBridge/HarnessBridgeHost.cs`
- `run-harness-scenario`, `dispatch-harness-node`, `inspect-harness-run`은 clean-boot 게이트 전까지 명시적으로 비활성화했다. bridge와 scenario/policy 계층을 다시 분리하기 위한 올바른 후퇴다. 근거: `src/Sts2ModKit.Tool/HarnessCommands.cs`
- actuator를 다시 열지 않은 채 `arm/disarm/inspect`만 남겨 manual clean-boot triage 표면을 단순화했다. 근거: `src/Sts2ModKit.Tool/HarnessCommands.cs`

## 3. 위험한 점
- `2026-03-13` 기준 새 최소 bridge로 `Manual Clean Boot`를 다시 통과시켰다. stale action은 `action-ignored`로만 남았고 live state는 `main-menu`에 머물렀다.
- `dispatch_node`와 `inventory.latest.json` 경로를 잠시 꺼 두었기 때문에 외부 AI policy 연동 준비도는 이번 순간에 오히려 낮아졌다. 이는 의도적 후퇴지만 문서와 기대치를 분리해서 관리해야 한다.
- `BridgeActionExecutor`와 scripted scenario stack은 코드상 남아 있지만 clean-boot cycle에서는 사실상 죽은 경로다. 다음 사이클에서 policy adapter 뒤로 숨기거나 더 강하게 분리해야 한다.
- `actions.ndjson`에 대한 dedupe는 현재 프로세스 메모리의 `_seenActionIds`에 의존한다. clean-boot 복구에는 충분하지만, bridge 재시작 후 ordering/idempotency 보장은 아직 약하다.
- `results.ndjson`를 이번 사이클에서 적극적으로 생산하지 않으므로, arm 이후 actuation acceptance를 논하기엔 아직 이르다.
- review 문서 기준 P0 중 `combat direct call 제거`는 이번 사이클에서 간접적으로 봉인했지만, 관련 경로가 코드베이스 전체에서 완전히 제거된 것은 아니다. post-clean-boot cycle에서 확실히 정리해야 한다.

## 4. 체크리스트
- [~] 조작 표면: 이번 사이클에서는 actuator를 의도적으로 닫았다. legacy semantic action을 runtime bridge에서 비활성화하는 방향은 맞지만, 최종 조작 표면은 아직 재개방 전이다.
- [~] observer: live export 기반 scene/state 관측은 유지된다. 다만 harness inventory observer는 clean-boot 사이클 동안 잠시 꺼 두었다.
- [~] actuator: `dispatch_node`와 semantic action 경로를 이번 사이클에서 비활성화했다. 안전성은 올라갔지만 actuator readiness 자체는 아직 미완이다.
- [~] preflight/postflight: 이번 사이클에서는 action 자체를 실행하지 않으므로 dispatch용 preflight/postflight는 다음 사이클 과제다.
- [x] session/guard: `arm.json` 기반 dormant/armed 전환과 arm 없을 때 action 미소비 경계를 clean-boot 전용 bridge로 다시 고정했다.
- [~] file contract: `status.json` atomic write는 반영됐고 clean boot에서 정상 기록을 확인했다. 하지만 action/result ordering, idempotency, inventory/result contract는 아직 보강이 더 필요하다.
- [~] replay: run artifact와 replay 관련 코드는 남아 있지만 clean-boot 기준 복구에는 아직 직접 기여하지 않는다.
- [~] smoke/progression suitability: `Manual Clean Boot`는 통과했다. scripted smoke/progression 경로는 여전히 의도적으로 비활성화된 상태다.
- [~] manual control readiness: `arm/disarm/inspect`는 유지된다. `dispatch`와 scenario 실행은 의도적으로 막아 두었다.
- [~] future AI policy readiness: `inventoryId + nodeId + sessionToken` 방향성은 유지하지만, 현재 구현은 clean-boot 우선 복구 단계다.

## 5. 우선순위

### P0
- 새 최소 bridge 기준으로 `Manual Clean Boot` acceptance를 실제로 통과시켜야 한다.
- stale `actions.ndjson`가 남아 있을 때 `action-ignored`만 남고 자동 진행이 발생하지 않는지 검증해야 한다.
- `status.json`이 부팅 직후 안정적으로 생성되고 `mode=dormant`로 보이는지 확인해야 한다.
- clean-boot acceptance가 닫히면 그 시점의 trace/status/runtime log를 기준 artifact로 보존해야 한다.

### P1
- `inventory.latest.json` observer를 다시 열되, actuation 없이 publish-only로 먼저 복구해야 한다.
- `dispatch_node`는 inventory match뿐 아니라 preflight/postflight를 강제하는 구조로 재도입해야 한다.
- scenario/policy 계층은 bridge 직접 실행 경로와 분리된 adapter 뒤로 옮겨야 한다.
- `BridgeActionExecutor`와 result schema를 clean-boot 이후 contract에 맞춰 다시 정리해야 한다.

### P2
- operator용 inspect/manual control UX를 보강해야 한다. 최소한 clean-boot triage에 필요한 요약 출력은 더 좋아질 수 있다.
- replay bundle에 status/trace/action lifecycle을 더 쉽게 묶는 방법을 추가해야 한다.
- screenshot은 계속 보조 센서로만 다루고, primary control channel은 structured state + inventory로 유지해야 한다.

## 6. 리팩토링 제안
- `HarnessBridgeHost`는 계속 최소 orchestration class로 유지하고, 이후에도 책임을 다시 한 클래스에 합치지 않는다.
- actuator를 재도입할 때는 `InventoryPublisher`, `ActionValidator`, `UiDispatcher`, `TransitionVerifier`를 별도 구성요소로 둔다.
- `click_card`와 유사한 combat direct path는 runtime 기본 경로에서 금지하고, 남기더라도 explicit unsafe fallback 뒤로 격리한다.
- external contract는 다시 열 때도 `dispatch_node` 중심으로만 복구한다. `TargetLabel`과 sentinel alias는 로그 힌트로만 남긴다.
- trace 구조는 `bridge-started -> mode-changed -> action-seen -> action-ignored/rejected`에서 시작해, 이후 `validated -> dispatched -> observed -> completed` lifecycle로 확장한다.

## 판단 요약
- 현재 구조는 이제 “기능 많은 불안정 하네스”에서 “기준선 복구를 위한 최소 clean-boot bridge”로 의도적으로 축소됐다.
- 이 축소는 후퇴가 아니라 acceptance 기준선 복구를 위한 정리 단계다.
- 다음 게이트는 오직 `Manual Clean Boot`다. 이것을 통과하기 전까지 `dispatch_node`, scripted scenario, unattended harness 결과는 다시 신뢰하지 않는다.


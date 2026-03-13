# STS2 Harness Review

## 1. 현재 구조 요약
- observer: `LiveExportStateTracker -> live export files -> CompanionStateMapper -> LiveCompanionStateSource` 경로가 여전히 기본 world model이다. 이제 harness bridge도 같은 live export snapshot을 읽어 `inventory.latest.json`를 publish-only로 다시 발행한다. `2026-03-13` Steam URI clean boot에서 dormant 상태로 실제 파일 생성까지 확인했다. actuator와 분리된 observer-only 복구로 보는 편이 맞다. 평가: `Improving`
- actuator: 이번 사이클의 bridge는 의도적으로 actuator를 열지 않는다. `arm.json`이 있어도 action은 실행하지 않고 `action-rejected`로 남긴다. 평가는 기능 부족이 아니라 clean-boot 기준선 복구를 위한 의도적 축소다. 평가: `Deferred`
- contract: 파일 기반 harness contract는 유지된다. 이번 사이클에서 실제로 닫힌 핵심은 `arm.json`, `actions.ndjson`, `status.json`, `trace.ndjson`이며, `inventory.latest.json`는 observer-only로 재개했다. `results.ndjson`는 여전히 post-clean-boot cycle로 보류했다. 평가: `OK`
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
- `inventory.latest.json`를 direct UI dispatch 없이 live export snapshot 기반으로 다시 발행하게 만들어 external observer 경로를 조금씩 복구하기 시작했다. 근거: `src/Sts2ModAiCompanion.HarnessBridge/InventoryPublisher.cs`, `src/Sts2ModAiCompanion.Mod/Runtime/RuntimeExportContext.cs`
- `2026-03-13` 재검증에서 Steam URI clean boot 후 `inventory.latest.json`가 dormant mode로 생성됐고, live state와 같은 `main-menu` choice set을 반영하는 것을 확인했다. stale action은 여전히 `action-ignored`로만 남았다.

## 3. 위험한 점
- `2026-03-13` 기준 새 최소 bridge로 `Manual Clean Boot`를 다시 통과시켰다. stale action은 `action-ignored`로만 남았고 live state는 `main-menu`에 머물렀다.
- `inventory.latest.json`는 publish-only로 복구됐지만, source-of-truth가 live export snapshot이라서 아직 실제 UI node tree와 1:1 계약은 아니다. external AI policy 연동 준비도는 이전보다 좋아졌지만, actuation acceptance 기준으로 보기엔 아직 부족하다.
- 초기 poll에서 `bootstrap -> combat -> main-menu`처럼 transient sceneType가 잠깐 기록된다. 현재 publish-only observer는 steady-state triage에는 충분하지만, 이후 `dispatch_node` 재개 전에는 scene stabilization 규칙이 더 필요하다.
- `BridgeActionExecutor`와 scripted scenario stack은 코드상 남아 있지만 clean-boot cycle에서는 사실상 죽은 경로다. 다음 사이클에서 policy adapter 뒤로 숨기거나 더 강하게 분리해야 한다.
- `actions.ndjson`에 대한 dedupe는 현재 프로세스 메모리의 `_seenActionIds`에 의존한다. clean-boot 복구에는 충분하지만, bridge 재시작 후 ordering/idempotency 보장은 아직 약하다.
- `results.ndjson`를 이번 사이클에서 적극적으로 생산하지 않으므로, arm 이후 actuation acceptance를 논하기엔 아직 이르다.
- review 문서 기준 P0 중 `combat direct call 제거`는 이번 사이클에서 간접적으로 봉인했지만, 관련 경로가 코드베이스 전체에서 완전히 제거된 것은 아니다. post-clean-boot cycle에서 확실히 정리해야 한다.

## 4. 체크리스트
- [~] 조작 표면: 이번 사이클에서는 actuator를 의도적으로 닫았다. legacy semantic action을 runtime bridge에서 비활성화하는 방향은 맞지만, 최종 조작 표면은 아직 재개방 전이다.
- [~] observer: live export 기반 scene/state 관측은 유지된다. harness inventory observer도 publish-only로 다시 열었고 clean boot에서 실제 생성까지 확인했지만, 아직 live export-derived inventory 수준이다.
- [~] actuator: `dispatch_node`와 semantic action 경로를 이번 사이클에서 비활성화했다. 안전성은 올라갔지만 actuator readiness 자체는 아직 미완이다.
- [~] preflight/postflight: 이번 사이클에서는 action 자체를 실행하지 않으므로 dispatch용 preflight/postflight는 다음 사이클 과제다.
- [x] session/guard: `arm.json` 기반 dormant/armed 전환과 arm 없을 때 action 미소비 경계를 clean-boot 전용 bridge로 다시 고정했다.
- [~] file contract: `status.json` atomic write는 반영됐고 clean boot에서 정상 기록을 확인했다. `inventory.latest.json`도 observer-only로 다시 발행한다. 하지만 action/result ordering, idempotency, dispatch/result contract는 아직 보강이 더 필요하다.
- [~] replay: run artifact와 replay 관련 코드는 남아 있지만 clean-boot 기준 복구에는 아직 직접 기여하지 않는다.
- [~] smoke/progression suitability: `Manual Clean Boot`는 통과했고 publish-only inventory observer도 clean boot에서 동작을 확인했다. scripted smoke/progression 경로는 여전히 의도적으로 비활성화된 상태다.
- [~] manual control readiness: `arm/disarm/inspect`는 유지된다. `dispatch`와 scenario 실행은 의도적으로 막아 두었다.
- [~] future AI policy readiness: `inventoryId + nodeId + sessionToken` 방향성은 유지한다. inventory publish-only는 다시 열었지만, actuator와 preflight/postflight는 아직 닫혀 있다.

## 5. 우선순위

### P0
- 새 최소 bridge 기준으로 `Manual Clean Boot` acceptance를 실제로 통과시켜야 한다.
- stale `actions.ndjson`가 남아 있을 때 `action-ignored`만 남고 자동 진행이 발생하지 않는지 검증해야 한다.
- `status.json`이 부팅 직후 안정적으로 생성되고 `mode=dormant`로 보이는지 확인해야 한다.
- clean-boot acceptance가 닫히면 그 시점의 trace/status/runtime log를 기준 artifact로 보존해야 한다.

### P1
- [완료] `inventory.latest.json` observer를 actuation 없이 publish-only로 먼저 복구했고, Steam URI clean boot에서 dormant 상태 파일 생성까지 확인했다.
- 다음은 inventory fidelity와 scene-specific node typing을 높이고 transient scene stabilization을 넣는 일이다.
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


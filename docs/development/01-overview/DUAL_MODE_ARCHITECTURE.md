# Dual-Mode 아키텍처

이 저장소는 이제 단일 advisor 앱이 아니라, 공통 foundation 위에 `advisor mode`와 `harness mode`가 공존하는 구조로 해석합니다.

## 1. 최상위 모드

### Production Mode
- 사용자가 직접 STS2를 플레이합니다.
- AI는 게임 밖의 별도 프로세스에서 상태를 읽습니다.
- 조언은 하지만 입력을 넣지 않습니다.
- 핵심 키워드: `external observer`, `read-only`, `contextual advice`

### Test Mode
- 사람 없이도 최소 시나리오를 자동 진행하고 회귀를 평가합니다.
- foundation에서 나온 상태를 읽고 test-only action layer로 행동을 실행합니다.
- 핵심 키워드: `human-free automation`, `recovery`, `replay`, `regression`

## 2. 3층 구조

### Shared Foundation
- `src/Sts2ModKit.Core`
- `src/Sts2ModAiCompanion.Mod`
- `src/Sts2AiCompanion.Foundation`

공통 책임:
- runtime observation/export
- semantic state normalization
- knowledge extraction / slice
- session/run/artifact lifecycle
- collector / diagnostics
- Codex prompt/response contract

### Advisor Mode
- `src/Sts2AiCompanion.Advisor`
- `src/Sts2AiCompanion.Wpf`
- legacy migration shim: `src/Sts2AiCompanion.Host`

책임:
- read-only advice orchestration
- user-facing recommendation formatting
- WPF presentation
- `Analyze Now`, `Retry Last`, auto advice UX

### Harness Mode
- `src/Sts2AiCompanion.Harness`
- `src/Sts2ModAiCompanion.HarnessBridge`
- `scenarios/`
- `tests/replay-fixtures/`

책임:
- scenario runner
- deterministic policy
- action executor abstraction
- recovery / acceptance / replay

## 3. 현재 리팩터링 상태

이번 단계에서 실제로 반영된 것:
- shared contract와 상태 모델을 `Foundation`으로 추가
- `HarnessAction`과 harness skeleton 추가
- WPF가 legacy host 대신 advisor façade를 보도록 전환 시작
- harness bridge용 test-only ingress 프로젝트 추가

아직 남아 있는 migration:
- `Sts2AiCompanion.Host` 안의 shared service를 `Foundation`으로 완전히 이동
- tool/self-test가 advisor/foundation을 직접 쓰도록 재배선
- harness PoC의 실제 action execution 구현

## 4. 핵심 원칙

- 최종 제품은 여전히 read-only advisor입니다.
- harness는 production을 대체하지 않고 production 개발 속도를 올리기 위한 test mode입니다.
- mode별 계약은 분리하지만, `state`, `knowledge`, `session`, `artifact` vocabulary는 공유합니다.
- production UI는 foundation 위에 얹힌 surface이며, foundation이 아닙니다.

## Update (2026-03-13): observer/actuator split 구체화

- HarnessBridge는 이제 `observer + actuator` 경계로 해석합니다.
- bridge load 가능 상태와 actuator active 상태를 분리합니다.
- `harness.enabled`는 bridge를 로드할 수 있다는 뜻이지, 곧바로 action 소비를 허용한다는 뜻이 아닙니다.
- observer 역할은 dormant 상태에서도 유지될 수 있습니다. 즉 harness inventory export는 가능하지만 queue 소비와 test mode 활성화는 arm 이후에만 허용됩니다.
- external commander는 게임 밖에서 screenshot + live export + harness inventory를 보고 판단하고, DLL은 `nodeId` 기반 UI-bound event dispatch만 수행합니다.
- production advisor / WPF에는 이 harness control concern을 넣지 않습니다.

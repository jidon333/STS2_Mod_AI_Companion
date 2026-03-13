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

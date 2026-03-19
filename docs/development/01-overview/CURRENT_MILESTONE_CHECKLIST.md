# 현재 마일스톤 체크리스트

## 문서 역할

이 문서는 더 이상 상태를 길게 중복 설명하지 않는다.

- 장기 milestone 정의: [ROADMAP.md](../../ROADMAP.md)
- 현재 active milestone과 blocker: [PROJECT_STATUS.md](./PROJECT_STATUS.md)
- 다음 구현 세션용 지시: [AI_HANDOFF_PROMPT_KO.md](./AI_HANDOFF_PROMPT_KO.md)
- authoritative done semantics: [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](../05-harness/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)
- 실행 전후 운영 체크: [SMOKE_TEST_CHECKLIST.md](../../SMOKE_TEST_CHECKLIST.md), [AGENTS.md](../../../AGENTS.md)

즉, 이 파일의 역할은 `현재 운영 포인터를 짧게 모아두는 shortcut`이다.

## 현재 포인터

- 전체 목표: `Phase 1: 외부 프로세스 AI 조언 어시스턴트 완성`
- 현재 active milestone: `M2. 모드 로드 진입 증명`
- 현재 실제 blocker: `startup/load-chain`
- 다음 milestone: `M3. 런타임 부트스트랩 가동`

## 지금 세션에서 확인할 것

1. 현재 포인터와 blocker는 [PROJECT_STATUS.md](./PROJECT_STATUS.md)에서 본다.
2. 다음 구현 세션이 바로 해야 할 일은 [AI_HANDOFF_PROMPT_KO.md](./AI_HANDOFF_PROMPT_KO.md)에서 본다.
3. session done / milestone done 판정은 [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](../05-harness/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)를 따른다.
4. 실행 전후 체크는 [SMOKE_TEST_CHECKLIST.md](../../SMOKE_TEST_CHECKLIST.md)와 [AGENTS.md](../../../AGENTS.md)를 따른다.

## 읽는 순서

1. [PROJECT_STATUS.md](./PROJECT_STATUS.md)
2. [AI_HANDOFF_PROMPT_KO.md](./AI_HANDOFF_PROMPT_KO.md)
3. [ROADMAP.md](../../ROADMAP.md)
4. [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](../05-harness/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)

## 유지 원칙

- current blocker나 active milestone이 바뀌면 이 파일은 한두 줄 포인터만 갱신한다.
- 상세 상태표와 설명은 [PROJECT_STATUS.md](./PROJECT_STATUS.md)에만 둔다.
- 같은 내용을 `PROJECT_STATUS.md`와 이 파일에 이중으로 적지 않는다.

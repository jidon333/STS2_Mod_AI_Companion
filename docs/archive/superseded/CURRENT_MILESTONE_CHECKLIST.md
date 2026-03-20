# 현재 마일스톤 체크리스트

> Status: Archive
> Source of truth: No
> Superseded by [PROJECT_STATUS.md](../../current/PROJECT_STATUS.md) and [AI_HANDOFF_PROMPT_KO.md](../../current/AI_HANDOFF_PROMPT_KO.md).

## 문서 역할

이 문서는 더 이상 상태를 길게 중복 설명하지 않는다.

- 장기 milestone 정의: [ROADMAP.md](../../ROADMAP.md)
- 당시 active milestone과 blocker: [PROJECT_STATUS.md](../../current/PROJECT_STATUS.md)
- 다음 구현 세션용 지시: [AI_HANDOFF_PROMPT_KO.md](../../current/AI_HANDOFF_PROMPT_KO.md)
- authoritative done semantics: [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](../../contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)
- 실행 전후 운영 체크: [SMOKE_TEST_CHECKLIST.md](../../runbooks/SMOKE_TEST_CHECKLIST.md), [AGENTS.md](../../../AGENTS.md)

즉, 이 파일의 역할은 `당시 운영 포인터를 짧게 모아두는 shortcut`이다.

## 당시 포인터

- 전체 목표: `Phase 1: 외부 프로세스 AI 조언 어시스턴트 완성`
- 당시 active milestone: `M4. Trusted Attempt 확보`
- 당시 실제 blocker: `authoritative attempt / session accounting contract`
- 당시 다음 milestone: `M5. 하네스 장기 실행 증거 닫기`

## 지금 세션에서 확인할 것

1. first authoritative attempt가 `0001`이고 `trustStateAtStart:"valid"`로 시작하는지 [PROJECT_STATUS.md](../../current/PROJECT_STATUS.md) 기준으로 확인한다.
2. bootstrap launch가 `attempt-index.ndjson`와 `restart-events.ndjson`의 authoritative attempt accounting을 오염시키지 않는지 [AI_HANDOFF_PROMPT_KO.md](../../current/AI_HANDOFF_PROMPT_KO.md) 기준으로 본다.
3. `restart-events` / `attempt-index` / `session-summary` / `supervisor-state` 해석 규칙은 [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](../../contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)를 따른다.
4. 실행 전후 체크는 [SMOKE_TEST_CHECKLIST.md](../../runbooks/SMOKE_TEST_CHECKLIST.md)와 [AGENTS.md](../../../AGENTS.md)를 따른다.

## 읽는 순서

1. [PROJECT_STATUS.md](../../current/PROJECT_STATUS.md)
2. [AI_HANDOFF_PROMPT_KO.md](../../current/AI_HANDOFF_PROMPT_KO.md)
3. [ROADMAP.md](../../ROADMAP.md)
4. [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](../../contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)

## 유지 원칙

- 당시 blocker나 active milestone이 바뀌면 이 파일은 한두 줄 포인터만 갱신한다.
- 상세 상태표와 설명은 [PROJECT_STATUS.md](../../current/PROJECT_STATUS.md)에만 둔다.
- 같은 내용을 `PROJECT_STATUS.md`와 이 파일에 이중으로 적지 않는다.

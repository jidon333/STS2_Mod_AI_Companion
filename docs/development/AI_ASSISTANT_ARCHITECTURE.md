# AI 어시스턴트 아키텍처

현재 Phase 1의 최종 구조는 `게임 내부 exporter + 게임 외부 host + WPF UI + Codex CLI` 입니다.

## 1. 전체 흐름

1. 게임 안의 native 모드가 상태를 읽음
2. live export 4종 파일을 기록
3. host가 live export를 polling
4. host가 정적 지식 카탈로그를 로드
5. host가 knowledge slice를 선택
6. host가 prompt pack을 만들고 Codex CLI를 호출
7. advice 결과를 artifact로 저장
8. WPF 앱이 현재 상태와 advice를 표시

## 2. host의 책임

`Sts2AiCompanion.Host`는 아래 서비스를 담는 레이어입니다.

- live export watcher
- knowledge catalog service
- prompt builder
- Codex CLI client
- run/session coordinator
- advice artifact writer

## 3. artifact 구조

현재 companion artifact root는 아래입니다.

- `artifacts/companion/current-run.json`
- `artifacts/companion/<run-id>/live-mirror/`
- `artifacts/companion/<run-id>/prompt-packs/`
- `artifacts/companion/<run-id>/advice.ndjson`
- `artifacts/companion/<run-id>/advice.latest.json`
- `artifacts/companion/<run-id>/advice.latest.md`
- `artifacts/companion/<run-id>/codex-session.json`
- `artifacts/companion/<run-id>/host-status.json`

즉, run별로 무엇을 보냈고 무엇을 받았는지 추적할 수 있게 설계합니다.

## 4. prompt 계약

prompt pack은 아래를 담습니다.

- trigger kind
- current screen
- 상태 요약 텍스트
- 최근 이벤트 tail
- knowledge slice
- assistant constraints

중요:

- 전체 카탈로그를 매번 통째로 보내지 않습니다.
- 현재 화면과 선택지, 덱/유물/포션과 관련 있는 일부만 보냅니다.

## 5. advice 계약

Codex 응답은 JSON schema 기반으로 정리합니다.

핵심 필드:

- `headline`
- `summary`
- `recommendedAction`
- `recommendedChoiceLabel`
- `reasoningBullets`
- `riskNotes`
- `confidence`
- `knowledgeRefs`

## 6. degraded 동작

Codex가 실패해도 아래는 계속 유지해야 합니다.

- live state 표시
- current choices 표시
- 최근 이벤트 표시
- artifact 기록
- manual retry

즉, 게임과 조언 앱은 느슨하게 결합되어야 합니다.

## 7. 현재 남아 있는 작업

- 실제 gameplay와의 end-to-end smoke
- 자동 trigger 튜닝
- run 종료 summary 흐름
- replay harness 기반 자동 acceptance

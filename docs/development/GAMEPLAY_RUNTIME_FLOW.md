# 실제 플레이 런타임 흐름

이 문서는 게임을 켜고 플레이하는 동안 어시스턴트가 어떤 순서로 움직이는지 설명합니다.

## 1. 사용자가 어시스턴트 앱을 켠다

WPF 앱을 먼저 실행하면 내부적으로 `CompanionHost`가 같이 올라옵니다.

이때 Host가 하는 일:

- live export 경로 확인
- assistant catalog 로드
- polling loop 시작
- companion artifact root 준비
- Codex CLI 경로 확인

이 시점 기본 상태는 보통 `실시간 추출 대기 중`입니다.

## 2. 사용자가 게임을 실행한다

게임은 반드시 Steam URI로 실행합니다.

```powershell
cmd /c start "" "steam://rungameid/2868840"
```

게임이 켜지면 모드가 로드되고 runtime exporter가 초기화됩니다.

exporter가 하는 일:

- Harmony patch 적용
- runtime identity / hook summary 로그 기록
- scene polling 시작
- live export 파일 갱신

## 3. live export가 생성된다

기본 live export 4종:

- `events.ndjson`
- `state.latest.json`
- `state.latest.txt`
- `session.json`

collector mode가 켜져 있으면 추가로 아래가 생성됩니다.

- `raw-observations.ndjson`
- `screen-transitions.ndjson`
- `choice-candidates.ndjson`
- `choice-decisions.ndjson`
- `semantic-snapshots/*`

## 4. Host가 현재 런에 attach한다

Host는 최신 snapshot과 최근 events를 읽어 `CompanionRunState`를 만든 뒤, 현재 런 ID에 맞는 artifact 경로를 준비합니다.

주요 산출물:

- `artifacts/companion/current-run.json`
- `artifacts/companion/<run-id>/live-mirror/`
- `artifacts/companion/<run-id>/prompt-packs/`
- `artifacts/companion/<run-id>/advice/`
- `artifacts/companion/<run-id>/host-status.json`
- `artifacts/companion/<run-id>/codex-session.json`
- `artifacts/companion/<run-id>/codex-trace.ndjson`
- `artifacts/companion/<run-id>/collector-summary.json`

## 5. high-value 화면에서 trigger가 발생한다

현재 중요한 trigger:

- `choice-list-presented`
- `reward-screen-opened`
- `event-screen-opened`
- `shop-opened`
- `rest-opened`
- 수동 `manual`
- 수동 `retry-last`

정책:

- automatic advice는 high-value trigger만 사용
- `runtime-poll`은 상태 갱신용으로만 사용
- 같은 런에서는 Codex 요청 1개만 in-flight
- high-priority trigger는 latest-only coalesce

## 6. prompt pack이 만들어진다

`AdvicePromptBuilder`가 아래를 묶어 `AdviceInputPack`을 만듭니다.

- current snapshot
- recent events tail
- knowledge slice
- assistant constraints

`Analyze Now`는 현재 snapshot 기준 새 prompt를 만들고, `Retry Last`는 마지막 prompt pack을 다시 보냅니다. 둘은 이제 같은 의미가 아닙니다.

## 7. Codex에 조언을 요청한다

`CodexCliClient`는 현재 런의 `sessionId`가 있으면 resume하고, 없으면 `thread.started` 또는 세션 인덱스 기반으로 새 세션을 잡습니다.

현재까지 확인된 것:

- manual / automatic / retry 요청 모두 같은 run 기준 `sessionId`를 재사용하도록 설계됨
- run-scoped 세션 복구를 위해 `codex-session.json`을 다시 읽을 수 있음

아직 gameplay automatic trigger 기준으로는 추가 실증이 필요합니다.

## 8. advice artifact가 저장된다

성공 또는 degraded 결과는 아래로 남습니다.

- `advice/advice.latest.json`
- `advice/advice.latest.md`
- `advice/advice.ndjson`
- `codex-trace.ndjson`

collector mode가 켜져 있으면 `missingInformation`, `decisionBlockers`, knowledge refs, request latency 같은 진단도 같이 남습니다.

## 9. WPF가 상태와 조언을 표시한다

WPF가 보여주는 영역:

- 현재 상태
- 덱 요약
- 유물 / 포션
- AI 조언
- 현재 선택지
- 최근 이벤트
- 관련 지식
- 수집 런 진단

추가 상태:

- 모델 선택
- 추론 강도 선택
- 분석중 / 재시도중 / 실패 / 취소 / 제한 상태
- 분석 경과 시간

## 10. 플레이 종료 후 collector 후처리

수집 런이 끝나면 아래를 실행합니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- collector-postprocess --lines 200 --tail 40
```

이 후처리에서 요약하는 것:

- request latency summary
- duplicate/coalesced trigger summary
- screen overwrite summary
- state regression summary
- missing information top N
- decision blockers top N
- knowledge usage summary
- runtime fatal errors
- apphang suspicion indicators

## 11. 현재 남은 병목

- reward / event / shop / rest의 실제 선택지 텍스트 추출
- semantic screen 유지
- gameplay automatic advice
- gameplay trigger에서도 같은 session id 재사용
- AppHang 방어

# 프로젝트 상태

기준 시점:

- 날짜: `2026-03-11`
- 게임 버전: `STS2 v0.98.3`
- 주요 툴체인: `Godot 4.5.1`, `.NET 7`, `ilspycmd 8.2`

이 문서는 “지금 이 저장소가 실제로 어디까지 구현 / 검증됐는가”를 빠르게 보는 상태 문서입니다.

## 1. 현재 저장소의 중심

현재 저장소의 중심 기능은 아래 네 가지입니다.

1. 게임 내부 `read-only runtime exporter`
2. strict parser 기반 정적 지식 추출 파이프라인
3. 외부 `Host + Codex` 조언 경로
4. `WPF` 사용자 인터페이스

즉 자동 플레이 모드가 아니라, 사람이 플레이하는 동안 게임 밖에서 상태를 읽고 조언을 주는 외부 어시스턴트가 현재 목표입니다.

## 2. 현재 검증된 것

### 2.1 빌드와 기본 검증

현재 기준으로 아래는 통과 상태입니다.

- `dotnet build STS2_Mod_AI_Companion.sln`
- `dotnet run --project src\Sts2ModKit.SelfTest --no-build`
- `dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge`
- `dotnet run --project src\Sts2ModKit.Tool -- inspect-static-knowledge`

### 2.2 runtime exporter

현재 실제로 확인된 범위:

- 모드 로드
- Harmony startup failure 제거
- main menu까지 live export 생성
  - `events.ndjson`
  - `state.latest.json`
  - `state.latest.txt`
  - `session.json`
- reward / event / rest / shop hook observed
- scene polling 기반 상태 갱신

### 2.3 정적 지식 파이프라인

현재 canonical counts:

- cards: `576`
- relics: `288`
- potions: `63`
- events: `58`
- shops: `5`
- rewards: `7`
- keywords: `262`

현재 localization coverage:

- cards: `582` / descriptions: `569` / selection prompts: `22`
- relics: `285` / descriptions: `285`
- potions: `75` / descriptions: `70`
- events: `153` / descriptions: `60` / options: `203`
- shops: `4` / descriptions: `4`
- rewards: `3` / descriptions: `2`
- keywords: `245` / descriptions: `245`

### 2.4 Host / WPF

코드와 self-test 기준으로 존재 / 동작이 확인된 것:

- `CompanionHost` polling loop
- `KnowledgeCatalogService` knowledge slice 선택
- `AdvicePromptBuilder` prompt pack / markdown 생성
- `CodexCliClient` Codex CLI JSON 호출
- WPF UI 상태 / choices / recent events / knowledge / advice 표시
- manual advice 경로
  - `Analyze Now`
  - `dotnet run --project src\Sts2ModKit.Tool -- analyze-live-once`

## 3. 이번 단계에서 새로 반영된 것

### 3.1 gameplay extractor 보강

이번 턴에서 아래를 수정했습니다.

- overlay 화면이 `RoomType`보다 우선되도록 screen 판정 순서 조정
- `currentChoices` 후보 수집 범위 확대
  - `Choices`
  - `RewardButtons`
  - `RewardAlternatives`
  - `Hand`
  - `HandCards`
  - UI child node
  - `Option`, `Reward`, `Entry`, `Card`, `Model`
- `LocString`, `MegaLabel`, `MegaRichTextLabel`, `RichTextLabel`, `Text`, `BbcodeText`에서 표시 문자열 추출

### 3.2 Host 수동 조언 경로 보강

- manual advice 생성 직후 snapshot을 다시 publish하도록 수정
- `analyze-live-once` 실행 시 `host-status`와 latest snapshot에 `lastAdviceAt` / `Advice generated for manual.`가 즉시 반영되도록 수정

### 3.3 self-test 회귀 추가

새 self-test:

- nested label text extraction
- overlay screen precedence

## 4. 현재 확인된 Codex 상태

현재 사실은 아래처럼 나뉩니다.

### 성공한 것

- Codex CLI 호출 자체
- manual advice 응답 생성
- `advice.latest.json`
- `advice.latest.md`
- `advice.ndjson`

### 아직 닫히지 않은 것

- UTF-8 수정 이후 fresh automatic trigger 재검증
- `sessionId`를 안정적으로 잡아 run-scoped session을 이어 붙이는 경로

현재 최신 성공 advice에서도 `sessionId = null`이고 `codex-session.json`이 생성되지 않았습니다. 따라서 지금 단계의 정확한 표현은 “Codex 응답 생성은 성공, run-scoped session 추적은 미완료”입니다.

## 5. 현재 남은 리스크

### 5.1 gameplay smoke 리스크

main menu 이후 gameplay high-value 화면은 아직 실제 한 판으로 끝까지 재검증하지 않았습니다.

### 5.2 automatic advice 리스크

이전 degraded 원인은 UTF-8 stdin 문제였고 코드는 수정됐습니다. 하지만 수정 이후의 새 `choice-list-presented` / `reward-screen-opened` trigger를 아직 다시 발생시키지 못해, automatic advice 성공은 재실증이 필요합니다.

### 5.3 session tracking 리스크

Codex 응답은 오지만 `sessionId`가 비어 있습니다. 즉 런 단위 대화 세션을 계속 이어 붙이는 핵심 경로는 아직 완전히 닫히지 않았습니다.

### 5.4 screen / choice 품질 리스크

choice extraction은 좋아졌지만, reward / shop / rest 화면에서 라벨이 항상 완전히 사람 읽는 텍스트로 나오는지는 실제 gameplay smoke로 다시 확인해야 합니다.

## 6. 다음 우선순위

1. 실제 gameplay 한 판으로 reward / event / shop / rest / combat 중 최소 2개 이상 검증
2. automatic advice 재검증
3. Codex session id capture / resume 경로 보강
4. observed-merge와 assistant lookup을 runtime choice와 교차 검증

## 7. 한 줄 요약

현재 저장소는 `strict 정적 지식 + exporter + manual advice + WPF 뼈대`까지는 실사용에 가깝게 올라왔고, 다음 병목은 `실제 gameplay high-value 화면에서 auto advice와 session tracking을 닫는 것`입니다.

# 실제 플레이 런타임 흐름

이 문서는 게임을 켰을 때부터 외부 조언 앱이 정보를 읽고 advice artifact를 남기기까지 실제로 어떤 흐름으로 움직이는지 설명합니다.

## 1. 사용자가 외부 앱을 켭니다

WPF 앱을 먼저 실행하면 내부적으로 `CompanionHost`가 같이 올라옵니다.

이때 Host가 하는 일:

- live export 경로 확인
- assistant catalog 로드
- polling loop 시작
- companion artifact root 준비
- Codex CLI 경로 확인

이 시점 상태는 보통 `waiting-live-export` 또는 `running`이지만, 아직 게임이 안 켜졌으면 advice는 생성되지 않습니다.

## 2. 사용자가 게임을 실행합니다

게임은 반드시 Steam URI로 실행합니다.

```powershell
cmd /c start "" "steam://rungameid/2868840"
```

게임이 켜지면 모드가 로드되고 runtime exporter가 초기화됩니다.

exporter가 하는 일:

- Harmony patch 대상 선택
- runtime identity / hook summary 로그 기록
- scene polling 시작
- live export 파일 4종 생성 / 갱신

현재까지는 main menu까지 이 흐름이 실제로 확인됐습니다.

## 3. main menu에 도달하면

현재까지 실증된 기준선이 여기입니다.

이 시점에 확인된 것:

- `events.ndjson`
- `state.latest.json`
- `state.latest.txt`
- `session.json`

Host는 이 파일을 읽고 현재 화면을 main menu로 표시할 수 있습니다.

## 4. run을 시작하면

run이 시작되면 exporter는 현재 run의 상태를 snapshot으로 기록하고, Host는 새 run artifact root를 준비합니다.

관련 산출물:

- `artifacts/companion/current-run.json`
- `artifacts/companion/<run-id>/live-mirror/`
- `artifacts/companion/<run-id>/prompt-packs/`
- `artifacts/companion/<run-id>/advice.ndjson`
- `artifacts/companion/<run-id>/advice.latest.json`
- `artifacts/companion/<run-id>/advice.latest.md`
- `artifacts/companion/<run-id>/host-status.json`

현재는 `sessionId`를 안정적으로 잡지 못해서 `codex-session.json`은 항상 생성된다고 볼 수 없습니다.

## 5. 고가치 화면에서 무엇이 일어나야 하는가

목표 화면:

- reward
- event
- shop
- rest-site
- combat start / turn start

이 화면에 도달하면 이상적인 흐름은 아래입니다.

1. exporter가 해당 화면을 명시적으로 분류
2. `currentChoices`를 사람이 읽는 라벨로 추출
3. Host가 trigger를 감지
4. knowledge slice를 구성
5. prompt pack을 저장
6. Codex에 조언 요청
7. advice artifact 저장
8. WPF에 최신 advice 표시

## 6. 현재 실제로 확인된 gameplay 관련 상태

실제 artifact / runtime log 기준으로 확인된 것:

- reward / event / rest / shop hook observed
- `choice-list-presented` prompt pack 생성
- `analyze-live-once` manual advice 성공

아직 다시 확인해야 하는 것:

- 최신 extractor 기준 reward / event / shop / rest 라벨이 사람이 읽는 텍스트로 잘 잡히는지
- automatic advice가 UTF-8 수정 이후 새 trigger에서 `ok`로 생성되는지
- gameplay 중 session tracking이 실제로 이어 붙는지

## 7. manual advice와 automatic advice의 차이

현재 상태를 정확히 나누면:

### manual advice

성공했습니다.

- `Analyze Now`
- `dotnet run --project src\Sts2ModKit.Tool -- analyze-live-once`

이 경로는 실제 Codex 응답을 받아 `advice.latest.json` / `advice.latest.md`를 생성합니다.

### automatic advice

이전에는 degraded였습니다.

원인:

- prompt stdin이 invalid UTF-8로 들어가면서 Codex CLI가 비어 있는 응답을 반환

현재:

- UTF-8 인코딩 강제와 sanitize 경로를 넣었습니다.
- 하지만 수정 이후의 fresh gameplay trigger를 아직 다시 만들지 못했습니다.

따라서 auto advice는 “수정 완료, 재검증 대기” 상태입니다.

## 8. Codex session 상태

현재 중요한 사실:

- Codex CLI 호출은 성공
- advice artifact 생성은 성공
- `sessionId`는 아직 `null`

즉 현재는 “Codex와 실제 대화해 조언을 받는 것”은 되지만, “런마다 세션을 생성해서 계속 이어 붙이는 것”은 아직 완성되지 않았습니다.

## 9. 지금 다음 플레이에서 무엇을 검증해야 하는가

다음 짧은 검증 런에서 확인할 항목:

1. reward / event / shop / rest 중 최소 2개 화면에서 `screen`이 올바르게 분류되는지
2. `currentChoices` 라벨이 `RichTextLabel#...` 같은 객체명이 아니라 실제 텍스트로 잡히는지
3. auto advice가 `degraded`가 아니라 `ok`로 저장되는지
4. 가능하면 `sessionId`가 채워지는지

## 10. 관련 코드 위치

- exporter / extractor
  - `src/Sts2ModAiCompanion.Mod/Runtime`
- live export 계약
  - `docs/REALTIME_EXTRACTION.md`
- Host / advice path
  - `src/Sts2AiCompanion.Host`
- WPF UI
  - `src/Sts2AiCompanion.Wpf`
- gameplay 관련 상태 문서
  - `docs/development/PROJECT_STATUS.md`
- 사용자 흐름
  - `docs/development/WPF_USER_FLOW.md`

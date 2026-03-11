# 실제 플레이 기준 런타임 흐름

이 문서는 `게임을 켠 뒤 이 저장소의 각 구성 요소가 실제로 어떤 순서로 움직이는가`를 설명합니다.

## 1. 앱을 먼저 켰을 때

사용자가 WPF 앱을 먼저 켜면 `ShellViewModel`이 `CompanionHost`를 올립니다.

이때 실제로 일어나는 것:

- Host polling loop 시작
- live export 폴더 대기
- assistant catalog 로드 준비
- Codex CLI 경로와 companion artifact 루트 확인

이 상태는 `대기 중`이며, 아직 live export가 없으면 advice는 생성되지 않습니다.

## 2. 게임을 실행했을 때

게임이 Steam을 통해 실행되면, `mods` 폴더의 `.pck + .dll` 조합으로 모드가 로드됩니다.

여기서 native mod는 다음 일을 합니다.

- Harmony 패치 등록
- runtime exporter 초기화
- 현재 화면과 player/root manager 탐색
- live export 파일 생성 준비

이 단계가 실패하면 `godot.log`와 `runtime log`에 바로 흔적이 남습니다.

## 3. 메인 메뉴에 도달했을 때

현재까지 실증된 기준점이 여기입니다.

- exporter가 main menu를 감지합니다.
- `events.ndjson`, `state.latest.json`, `state.latest.txt`, `session.json`이 생성됩니다.
- Host는 이 변화를 감지하고 현재 화면이 main menu임을 앱에 반영합니다.

이 시점에서는 강한 조언보다는 연결 상태와 현재 화면 인식이 핵심입니다.

## 4. 런을 시작했을 때

이후부터는 조언 시스템이 본격적으로 의미를 갖습니다.

- run id가 생깁니다.
- deck, relic, potion, HP, gold, floor 같은 상태가 snapshot에 반영됩니다.
- Host는 run 전용 artifact 폴더를 준비합니다.
- bounded knowledge slice를 만들 준비를 합니다.
- Codex 세션을 생성하거나 이어붙일 준비를 합니다.

## 5. 보상 / 이벤트 / 상점 / 휴식 화면에서

Phase 1에서 가장 중요한 순간입니다.

- exporter가 현재 화면과 choices를 읽습니다.
- Host는 현재 choices와 정적 지식 카탈로그를 매칭합니다.
- `AdvicePromptBuilder`가 prompt pack을 만듭니다.
- `CodexCliClient`가 추천 선택과 이유를 요청합니다.
- WPF 앱이 추천과 근거를 보여줍니다.

즉, 실제 사용자 경험은 “게임 옆 창에서 현재 선택지에 대한 조언이 뜨는 것”입니다.

## 6. 전투 시작 / 턴 시작에서

전투 중에는 매 프레임 추적을 하지 않습니다.

- 전투 시작
- 턴 시작
- 중요한 choice 경계

이 경계에서만 상태를 다시 읽고 조언을 갱신합니다.

## 7. 런 종료에서

- Host는 run summary를 정리합니다.
- advice 기록과 prompt pack을 artifact로 남깁니다.
- 앱은 다음 런을 기다리는 상태로 돌아갑니다.

## 8. 현재 UI에서 실제로 할 수 있는 수동 조작

현재 WPF에는 아래 버튼이 있습니다.

- `Analyze Now`
  - 현재 상태 기준 즉시 수동 advice 요청
- `Retry Last`
  - 현재 구현에서는 `Analyze Now`와 같은 경로로 다시 요청
- `Pause Auto Advice` / `Resume Auto Advice`
  - 자동 trigger on/off
- `Refresh Knowledge`
  - 현재 snapshot/knowledge 표시 갱신
- `Open Artifacts`
  - 현재 run 또는 companion root 열기

## 9. 현재 실증된 것과 아직 남은 것

### 실증된 것

- main menu까지의 live export 생성
- 정적 카탈로그 생성
- Host/WPF 빌드
- prompt/knowledge 계약 self-test

### 아직 남은 것

- reward/event/shop/rest/combat에서 choices가 실제로 안정적으로 잡히는지
- Codex advice가 실제 gameplay run에서 자동으로 생성되는지
- WPF가 그 결과를 실제 플레이 중 보여주는지

## 10. 이 문서를 어떤 코드와 같이 보면 좋은가

- mod 쪽 흐름: `src/Sts2ModAiCompanion.Mod/Runtime`
- live export 계약: `docs/REALTIME_EXTRACTION.md`
- 로드 체인 전체: `docs/development/LOAD_CHAIN.md`
- 구조 안내: `docs/development/REPO_STRUCTURE.md`
- 사용자 시나리오: `docs/development/WPF_USER_FLOW.md`

## 2026-03-11 플레이 흐름 보강 메모

실제 플레이 중 조언 경로는 이제 아래 두 축을 함께 봅니다.

- 실시간 축: `state.latest.json`, `state.latest.txt`, `events.ndjson`, `session.json`
- 정적 지식 축: `artifacts/knowledge/assistant/*.json`, `catalog.assistant.json`

즉, 이벤트/상점/보상 화면에서 조언이 생성될 때는 현재 선택지와 함께 해당 카드/유물/이벤트 설명 본문도 같이 참조하는 구조를 목표로 합니다.

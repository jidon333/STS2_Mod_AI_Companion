# 저장소 구조와 실제 플레이 흐름

이 문서는 `STS2_Mod_AI_Companion` 저장소를 단순한 폴더 목록이 아니라 `실제 슬더스2 플레이 중 무엇이 일어나는가` 기준으로 설명합니다.

## 1. 이 저장소가 실제로 하는 일

플레이어가 게임을 실행하면 이 저장소의 구성 요소는 아래 순서로 움직입니다.

1. 게임의 `mods` 폴더에서 `.pck + .dll` 조합으로 native mod가 로드됩니다.
2. 모드 안의 runtime exporter가 현재 화면, 덱, 유물, 포션, 선택지 같은 상태를 읽습니다.
3. 읽어낸 결과를 live export 파일 4종으로 씁니다.
   - `events.ndjson`
   - `state.latest.json`
   - `state.latest.txt`
   - `session.json`
4. 외부 Host가 이 파일 변화를 감시합니다.
5. Host는 정적 지식 카탈로그와 현재 live 상태를 합쳐서 AI 입력을 만듭니다.
6. WPF 앱이 현재 상태와 AI 조언을 사용자에게 보여줍니다.

중요한 점은, 이 저장소의 목표가 `게임을 대신 플레이하는 것`이 아니라 `플레이 중인 사람에게 조언을 주는 것`이라는 점입니다.

## 2. 실제 플레이 기준으로 보는 폴더 역할

### 루트

- `README.md`
  - 가장 먼저 보는 문서입니다.
  - 무엇을 구현하는 저장소인지, 어떻게 빌드하고 smoke test 하는지, 어떤 문서를 먼저 읽어야 하는지 설명합니다.
- `STS2_Mod_AI_Companion.sln`
  - 모드, 툴, self-test, Host, WPF를 묶는 솔루션입니다.
- `config/`
  - 게임 경로, Codex 경로, optional Godot 경로 같은 로컬 설정 샘플이 있습니다.

### `src/Sts2ModAiCompanion.Mod`

이 폴더는 게임 프로세스 안에서 실제로 로드되는 native mod입니다.

- 플레이 중 역할
  - 모드 로딩
  - Harmony 패치 등록
  - 런타임 상태 수집
  - live export 파일 기록
- 핵심 파일
  - `AiCompanionModEntryPoint.cs`
  - `Runtime/AiCompanionPatchStub.cs`
  - `Runtime/RuntimeHookCatalog.cs`
  - `Runtime/RuntimeExportContext.cs`
  - `Runtime/RuntimeSnapshotReflectionExtractor.cs`
- 이 폴더를 볼 때의 질문
  - 메인 메뉴를 왜 못 읽는가
  - 보상/이벤트/상점/휴식/전투 시작을 왜 못 잡는가
  - 어떤 데이터를 live export로 내보내는가

### `src/Sts2ModKit.Core`

모드, 툴, Host가 함께 쓰는 공용 로직입니다.

- 플레이 중 역할
  - live export 모델 정의
  - snapshot/restore 계획
  - smoke diagnostics
  - 정적 지식 카탈로그 조립
- 중요한 하위 폴더
  - `LiveExport/`
  - `Knowledge/`
  - `Diagnostics/`
  - `Planning/`
- 이 폴더를 볼 때의 질문
  - `state.latest.json` 구조는 무엇인가
  - static knowledge는 어떻게 합쳐지는가
  - smoke 결과를 어떻게 판정하는가

### `src/Sts2ModKit.Tool`

개발자용 CLI입니다. 게임 실행 전후 검증 루프를 담당합니다.

- 플레이 전/후 역할
  - `snapshot`
  - `restore`
  - `deploy-native-package`
  - `prepare-live-smoke`
  - `inspect-godot-log`
  - `inspect-live-export`
  - `extract-static-knowledge`
  - `inspect-static-knowledge`
- 이 폴더를 볼 때의 질문
  - smoke test를 어떤 순서로 돌려야 하는가
  - 정적 분석 산출물은 어떻게 생성되는가

### `src/Sts2ModKit.SelfTest`

실제 게임을 켜지 않고도 계약이 유지되는지 검사하는 프로젝트입니다.

- 플레이 전 역할
  - snapshot/restore 회귀 방지
  - live export writer 회귀 방지
  - prompt pack/knowledge slice 회귀 방지
- 이 폴더를 볼 때의 질문
  - 지금 바꾼 코드가 최소 계약을 깨지는 않았는가

### `src/Sts2AiCompanion.Host`

외부 조언 어시스턴트의 backend입니다.

- 플레이 중 역할
  - live export 폴더 감시
  - knowledge slice 선택
  - prompt pack 생성
  - Codex CLI 호출
  - advice artifact 저장
- 이 폴더를 볼 때의 질문
  - 어떤 화면에서 AI 호출이 트리거되는가
  - 어떤 live 상태와 어떤 정적 지식이 prompt에 들어가는가

### `src/Sts2AiCompanion.Wpf`

플레이어가 실제로 보게 될 외부 데스크톱 UI입니다.

- 플레이 중 역할
  - 현재 화면, 덱, 유물, 포션, 최근 변화 표시
  - AI 조언 표시
  - `Analyze Now` 같은 수동 조작 제공
- 이 폴더를 볼 때의 질문
  - 사용자는 어떤 화면을 보게 되는가
  - 조언이 실패해도 상태 표시는 계속 되는가

## 3. `artifacts/`는 실제로 무엇을 담는가

### `artifacts/knowledge`

정적 분석 결과입니다.

- `catalog.latest.json`
  - 기계가 읽는 canonical 카탈로그
- `catalog.latest.txt`
  - 빠르게 훑는 텍스트 요약
- `assembly-scan.json`
  - DLL 메타데이터 기반 후보
- `pck-inventory.json`
  - PCK 문자열 스캔 기반 후보
- `observed-merge.json`
  - 실플레이 관찰 결과 병합본
- `markdown/`
  - 사람이 읽기 좋은 Markdown 리포트

중요: 여기 있는 내용은 전부 실플레이 검증 완료본이 아닙니다. `무엇이 존재하는가`와 `어디에 연결되는가`를 먼저 파악하는 용도입니다.

### `artifacts/companion`

외부 assistant의 런별 산출물입니다.

- prompt pack
- latest advice
- advice history
- run별 live mirror

이 폴더를 보면 “AI가 실제로 무엇을 보고 어떤 답을 냈는가”를 추적할 수 있습니다.

### `artifacts/snapshots`

smoke test 전 백업본입니다.

- live game 폴더를 오염시키지 않기 위한 안전장치입니다.
- 문제가 생기면 이 폴더를 기준으로 restore 합니다.

### `artifacts/native-package-layout`

배포 전에 `.pck + .dll + runtime config` 조합이 어떻게 staging 되었는지 확인하는 폴더입니다.

## 4. 플레이 중 문제가 나면 어디를 볼까

### 게임이 실행됐는데 모드가 안 붙는 것 같을 때

1. `mods` 폴더의 배포 결과
2. `godot.log`
3. `inspect-godot-log`
4. `Runtime/AiCompanionPatchStub.cs`
5. `RuntimeHookCatalog.cs`

### 메인 메뉴는 되는데 보상/이벤트/상점을 못 읽을 때

1. `state.latest.json`
2. `events.ndjson`
3. `inspect-live-export`
4. `RuntimeSnapshotReflectionExtractor.cs`
5. `RuntimeHookCatalog.cs`

### AI 조언이 안 뜰 때

1. `artifacts/companion/`
2. `src/Sts2AiCompanion.Host`
3. `src/Sts2AiCompanion.Wpf`
4. Codex CLI 설정

### 정적 분석 리포트가 이상할 때

1. `assembly-scan.json`
2. `pck-inventory.json`
3. `observed-merge.json`
4. `src/Sts2ModKit.Core/Knowledge`

## 5. 처음 코드를 읽는 순서

1. `README.md`
2. `docs/ARCHITECTURE.md`
3. `docs/development/GAMEPLAY_RUNTIME_FLOW.md`
4. `docs/development/LOAD_CHAIN.md`
5. `src/Sts2ModAiCompanion.Mod/Runtime/RuntimeExportContext.cs`
6. `src/Sts2ModAiCompanion.Mod/Runtime/RuntimeSnapshotReflectionExtractor.cs`
7. `src/Sts2ModKit.Tool/Program.cs`
8. `src/Sts2AiCompanion.Host/CompanionHost.cs`
9. `src/Sts2AiCompanion.Wpf/ShellViewModel.cs`

## 6. 지금 실제로 어디까지 됐는가

- 검증된 것
  - 모드 빌드
  - self-test
  - 정적 지식 카탈로그 생성
  - 메인 메뉴까지의 live export 기본 동작
- 아직 남은 것
  - reward/event/shop/rest/combat 고가치 화면 실증
  - current choices 실플레이 교차 검증
  - WPF + Host + Codex의 실제 gameplay end-to-end 검증

이 문서를 읽고 나면 “이 저장소가 어떤 폴더를 갖고 있는가”보다 먼저 “플레이 중 어떤 흐름을 구현하려는 저장소인가”가 머리에 들어와야 합니다.

## 2026-03-11 구조 보강 메모

- `artifacts/knowledge/markdown/`는 사람이 읽는 정적 지식 리포트 루트입니다.
- `artifacts/knowledge/assistant/`는 AI가 직접 읽는 정적 지식 export 루트입니다.
- 두 폴더는 같은 `catalog.latest.json` 계열을 바탕으로 생성되지만, 목적이 다릅니다.
  - markdown: 사람이 카드/유물/이벤트/상점 설명을 읽고 교차검증하는 용도
  - assistant: Host/Codex/WPF가 바로 지식 slice를 만들 때 읽는 용도

# 프로젝트 상태

기준 시점:

- 날짜: `2026-03-11`
- 기준 게임 버전: `STS2 v0.98.2`
- 기준 런타임: `Godot 4.5.1`, `.NET 7`

## 현재 저장소의 중심

이 저장소는 현재 `슬레이 더 스파이어 2용 외부 AI 조언 어시스턴트`를 만들기 위한 구현/검증 저장소입니다.

현재 중심 구성은 아래 5개입니다.

- 게임 내부의 read-only native 모드
- 실시간 상태를 밖으로 꺼내는 runtime exporter
- 정적 지식 카탈로그 추출 파이프라인
- 외부 프로세스 host 라이브러리
- WPF 데스크톱 조언 앱

## 현재 구현된 것

### 1. 모드/패키징/복구 파이프라인

- native `mods + pck + dll + runtime config` 패키지 경로 유지
- `snapshot`, `restore`, `restore-snapshot-state`, `verify-snapshot`
- `deploy-native-package`, `prepare-live-smoke`
- `inspect-godot-log`, `inspect-live-export`

### 2. runtime exporter

- `events.ndjson`
- `state.latest.json`
- `state.latest.txt`
- `session.json`

를 게임 user data 아래의 `modded/profileN/ai_companion/live`에 기록합니다.

현재 확인된 기준선:

- Harmony `PatchAll` startup failure는 이전 대비 정리되었습니다.
- broad generic 훅 대신 더 좁은 후보군으로 진단 중입니다.
- main menu까지는 live export 4종 생성과 polling 기반 screen 분류가 확인된 상태입니다.

### 3. 정적 지식 추출

현재 `extract-static-knowledge`는 아래 단계를 수행합니다.

- `release-scan`
- `assembly-scan`
- `pck-inventory`
- `observed-merge`
- `catalog-build`

2026-03-11 기준 최신 비파괴 실행 결과:

- `cards`: 6635
- `relics`: 2366
- `potions`: 1078
- `events`: 1411
- `shops`: 417
- `rewards`: 288
- `keywords`: 1756

즉, 관찰 기반 카탈로그가 비어 있어도 오프라인 스캔만으로도 상당한 후보 지식 인벤토리를 만들 수 있는 상태입니다.

### 4. host / Codex backend

새로 추가된 `src/Sts2AiCompanion.Host`는 아래 역할을 담당합니다.

- live export polling
- run별 artifact 미러링
- knowledge slice 선택
- prompt pack 생성
- Codex CLI 세션 생성/재개
- advice artifact 저장

이 레이어는 이미 빌드되며 self-test에서 핵심 계약 일부를 검증합니다.

### 5. WPF UI

새로 추가된 `src/Sts2AiCompanion.Wpf`는 아래 패널을 가집니다.

- 현재 상태
- AI 조언
- 선택지 / 최근 이벤트 / 지식 슬라이스
- `Analyze Now`, `Pause Auto Advice`, `Retry Last`, `Refresh Knowledge`, `Open Artifacts`

현재 상태는 `빌드 가능`까지 확인되었고, 실제 live gameplay 전체와의 end-to-end 검증은 아직 남아 있습니다.

## 아직 남아 있는 핵심 작업

- reward / event / shop / rest / combat 화면 훅 실증
- `currentChoices`의 실제 gameplay 화면 반영 검증
- WPF 앱과 live gameplay의 end-to-end 검증
- replay 기반 자동 acceptance harness
- 실제 advice 품질 튜닝과 knowledge slice 정교화

## 현재 완료로 볼 수 없는 것

- 게임 안 모든 화면의 100% 안정적 추출
- 실제 조언 품질 완료
- 인게임 오버레이
- 자동 플레이
- 멀티플레이 동료

## 권장 다음 검증 순서

1. `dotnet build STS2_Mod_AI_Companion.sln`
2. `dotnet run --project src\Sts2ModKit.SelfTest`
3. `dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge`
4. `dotnet run --project src\Sts2ModKit.Tool -- inspect-static-knowledge`
5. live smoke가 필요하면 `snapshot -> deploy-native-package -> prepare-live-smoke -> Steam URI launch -> inspect-godot-log -> inspect-live-export`

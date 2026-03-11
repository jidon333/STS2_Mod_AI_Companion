# 저장소 구조 안내

이 문서는 현재 `STS2_Mod_AI_Companion` 저장소의 폴더 구조와 핵심 파일 역할을 빠르게 설명합니다.

## 1. 루트 폴더

### `README.md`

- 저장소의 현재 목적
- 빠른 시작
- 주요 프로젝트와 문서 링크

### `STS2_Mod_AI_Companion.sln`

- 전체 .NET 솔루션 파일입니다.
- Mod, Core, Tool, SelfTest, Host, WPF 프로젝트를 묶습니다.

### `config/`

- 로컬 개발과 검증에 쓰는 설정 샘플이 들어 있습니다.
- 핵심 파일: `ai-companion.sample.json`

### `docs/`

- 현재 구조, 범위, 검증 절차 같은 상위 문서가 들어 있습니다.

### `docs/development/`

- 작업 과정, 초보자용 설명, 조사 로그, 구조 설명을 담는 개발 문서 폴더입니다.

### `artifacts/`

- 빌드 산출물과 검증 결과를 모아 두는 폴더입니다.
- knowledge, snapshots, native-package-layout, companion 산출물이 여기 들어옵니다.

## 2. `src/` 아래 프로젝트

### `src/Sts2ModAiCompanion.Mod`

게임 안에 로드되는 native 모드입니다.

핵심 파일:

- `AiCompanionModEntryPoint.cs`
- `NativeModPackaging.cs`
- `Runtime/AiCompanionPatchStub.cs`
- `Runtime/RuntimeHookCatalog.cs`
- `Runtime/RuntimeExportContext.cs`
- `Runtime/RuntimeSnapshotReflectionExtractor.cs`
- `Runtime/AiCompanionRuntimeIdentity.cs`

### `src/Sts2ModKit.Core`

모드, 로컬 툴, 외부 host가 함께 쓰는 공용 코어입니다.

주요 하위 폴더:

- `Configuration/`
- `Planning/`
- `LiveExport/`
- `Knowledge/`
- `Diagnostics/`

### `src/Sts2ModKit.Tool`

개발자용 CLI입니다.

대표 명령:

- `show-config`
- `snapshot`
- `restore`
- `deploy-native-package`
- `prepare-live-smoke`
- `inspect-godot-log`
- `inspect-live-export`
- `extract-static-knowledge`
- `inspect-static-knowledge`

핵심 파일:

- `Program.cs`
- `LiveSmokeCommands.cs`
- `StaticKnowledgeCommands.cs`

### `src/Sts2ModKit.SelfTest`

비게임 환경에서 돌리는 회귀 테스트 프로젝트입니다.

현재 커버 범위:

- 설정 로더
- snapshot/restore
- 패키징
- live export tracker
- smoke diagnostics
- companion path resolver
- knowledge slice
- prompt builder

### `src/Sts2AiCompanion.Host`

외부 조언 어시스턴트의 backend 라이브러리입니다.

핵심 파일:

- `CompanionHost.cs`
- `CodexCliClient.cs`
- `KnowledgeCatalogService.cs`
- `AdvicePromptBuilder.cs`
- `CompanionPathResolver.cs`
- `CompanionModels.cs`

### `src/Sts2AiCompanion.Wpf`

최종 사용자용 WPF 데스크톱 UI입니다.

핵심 파일:

- `MainWindow.xaml`
- `MainWindow.xaml.cs`
- `ShellViewModel.cs`

## 3. `artifacts/` 주요 하위 폴더

### `artifacts/knowledge`

정적 지식 추출 결과가 들어 있습니다.

핵심 파일:

- `catalog.latest.json`
- `catalog.latest.txt`
- `source-manifest.json`
- `assembly-scan.json`
- `pck-inventory.json`
- `observed-merge.json`
- `markdown/`
  - 사람이 읽기 좋은 Markdown 리포트

### `artifacts/snapshots`

- smoke 전 snapshot 백업 결과

### `artifacts/native-package-layout`

- native 패키지 스테이징 결과

### `artifacts/companion`

- 외부 assistant의 run별 prompt, advice, live mirror artifact

## 4. 처음 코드를 읽을 때 추천 순서

1. `README.md`
2. `docs/ARCHITECTURE.md`
3. `docs/development/LOAD_CHAIN.md`
4. `src/Sts2ModAiCompanion.Mod/Runtime/RuntimeExportContext.cs`
5. `src/Sts2ModKit.Core/LiveExport/*`
6. `src/Sts2ModKit.Tool/Program.cs`
7. `src/Sts2AiCompanion.Host/CompanionHost.cs`
8. `src/Sts2AiCompanion.Wpf/ShellViewModel.cs`

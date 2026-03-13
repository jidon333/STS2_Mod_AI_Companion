# 초보자를 위한 빠른 입문 가이드

## 1. 이 저장소에서 가장 먼저 볼 것

- `README.md`
- `docs/development/README.md`
- `docs/development/01-overview/PROJECT_STATUS.md`

이 세 파일을 먼저 보면 현재 저장소가 무엇을 하고 있는지 큰 그림을 잡을 수 있습니다.

## 2. 코드 위치 빠른 안내

### 게임 내부 모드

- `src/Sts2ModAiCompanion.Mod`

여기서 중요한 파일:

- `AiCompanionModEntryPoint.cs`
- `NativeModPackaging.cs`
- `Runtime/RuntimeExportContext.cs`
- `Runtime/RuntimeHookCatalog.cs`
- `Runtime/RuntimeSnapshotReflectionExtractor.cs`
- `Runtime/AiCompanionPatchStub.cs`

### 공용 코어 / 툴링

- `src/Sts2ModKit.Core`
- `src/Sts2ModKit.Tool`

여기서 중요한 파일:

- snapshot/restore planning
- live export model
- smoke diagnostics
- static knowledge builder

### 외부 조언 앱

- `src/Sts2AiCompanion.Host`
- `src/Sts2AiCompanion.Wpf`

## 3. 가장 자주 쓰는 명령

```powershell
dotnet build STS2_Mod_AI_Companion.sln
dotnet run --project src\Sts2ModKit.SelfTest
dotnet run --project src\Sts2ModKit.Tool -- show-config
dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge
dotnet run --project src\Sts2ModKit.Tool -- inspect-static-knowledge
```

live smoke 전후 핵심 명령:

```powershell
dotnet run --project src\Sts2ModKit.Tool -- snapshot
dotnet run --project src\Sts2ModKit.Tool -- deploy-native-package
dotnet run --project src\Sts2ModKit.Tool -- prepare-live-smoke
cmd /c start "" "steam://rungameid/2868840"
dotnet run --project src\Sts2ModKit.Tool -- inspect-godot-log --lines 200
dotnet run --project src\Sts2ModKit.Tool -- inspect-live-export --tail 20
```

collector mode를 켰다면 플레이 후 아래도 바로 봅니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- collector-postprocess --lines 200 --tail 40
```

## 4. 빌드가 되는데 게임에서 안 되면 어디를 보나

1. `%AppData%\SlayTheSpire2\logs\godot.log`
2. 게임 `mods` 폴더의 `sts2-mod-ai-companion.runtime.log`
3. `inspect-live-export`
4. collector mode였다면 `collector-postprocess`

## 5. 작업 전 꼭 기억할 것

- direct exe 실행보다 Steam URI를 우선합니다.
- live smoke 전 snapshot을 만드는 습관을 유지합니다.
- exporter는 read-only 경계를 지킵니다.
- broad global 훅은 피합니다.
- high-value 화면 smoke를 다시 돌리기 전에는 `deploy-native-package`를 다시 수행해야 할 수 있습니다.
- `Analyze Now`와 `Retry Last`는 다릅니다.
  - `Analyze Now`: 현재 상태 기준 새 분석
  - `Retry Last`: 마지막 prompt pack 재전송

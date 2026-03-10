# 스모크 테스트 체크리스트

live runtime 검증은 아래 순서를 기준으로 진행합니다.

1. `dotnet run --project src\Sts2ModKit.Tool -- show-config`
2. `dotnet run --project src\Sts2ModKit.Tool -- snapshot`
3. 필요할 때만 `dotnet run --project src\Sts2ModKit.Tool -- build-native-pck --godot-exe <path>`
4. `dotnet run --project src\Sts2ModKit.Tool -- deploy-native-package`
5. `dotnet run --project src\Sts2ModKit.Tool -- prepare-live-smoke`
6. Steam URI로 게임 실행
   - `cmd /c start "" "steam://rungameid/2868840"`
7. main menu가 뜰 때까지 기다립니다.
8. 가능하면 run을 시작하고 high-value 화면 1~2개를 더 방문합니다.
9. `dotnet run --project src\Sts2ModKit.Tool -- inspect-godot-log --lines 200`
10. `dotnet run --project src\Sts2ModKit.Tool -- inspect-live-export --tail 20`
11. live export가 건강하면 `dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge`

## 최소 성공 신호

- loader crash 없이 게임이 부팅됨
- 게임 `mods` 폴더에 `sts2-mod-ai-companion.runtime.log`가 생성됨
- runtime log에 exporter startup identity와 hook summary가 남음
- `events.ndjson`, `state.latest.json`, `state.latest.txt`, `session.json` 4종이 live root에 생성됨
- `events.ndjson`가 append-only 한 줄 JSON 기록을 유지함
- `state.latest.txt`에 고정 섹션이 들어 있음
- `godot.log`에 manifest mismatch나 assembly load failure가 없음
- `inspect-godot-log` 결과에 `HarmonyPatchAllStartupFailure`가 없음
- `inspect-live-export` 결과에 `DeployedVsLiveRootMismatch`, `RuntimeLogMissing`이 없음

## 실패했을 때 먼저 볼 것

- `mods\sts2-mod-ai-companion.runtime.log`
- `%AppData%\SlayTheSpire2\logs\godot.log`
- `artifacts\native-package-layout\flat\native-deploy-report.json`
- 필요 시 최신 snapshot에서 `restore-snapshot-state`

# 스모크 테스트 체크리스트

> Status: Live Runbook
> Source of truth: Yes, for smoke procedure and success/failure signals.
> Update when: commands, launch flow, or acceptance signals change.

live runtime 검증은 아래 순서를 기준으로 진행합니다.

1. `dotnet run --project src\Sts2ModKit.Tool -- show-config`
2. `dotnet run --project src\Sts2ModKit.Tool -- snapshot`
3. 필요할 때만 `dotnet run --project src\Sts2ModKit.Tool -- build-native-pck --godot-exe <path>`
4. `dotnet run --project src\Sts2ModKit.Tool -- deploy-native-package`
5. `dotnet run --project src\Sts2ModKit.Tool -- prepare-live-smoke`
6. Steam URI로 게임 실행
   - `cmd /c start "" "steam://rungameid/2868840"`
7. main menu가 뜰 때까지 대기
8. 가능하면 run을 시작하고 high-value 화면 2~3개 방문
   - reward
   - event
   - rest
   - shop
9. `dotnet run --project src\Sts2ModKit.Tool -- inspect-godot-log --lines 200`
10. `dotnet run --project src\Sts2ModKit.Tool -- inspect-live-export --tail 20`
11. collector mode를 켰다면 `dotnet run --project src\Sts2ModKit.Tool -- collector-postprocess --lines 200 --tail 40`
12. 필요 시 `dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge`

## 최소 성공 신호

- loader crash 없이 게임이 부팅됨
- 게임 `mods` 폴더에 `sts2-mod-ai-companion.runtime.log`가 생성됨
- runtime log에 exporter startup identity, core dll identity, writer compatibility, hook summary가 남음
- runtime log에 `Method not found: LiveExportAtomicFileWriter.WriteJsonAtomic`가 없음
- `events.ndjson`, `state.latest.json`, `state.latest.txt`, `session.json` 4종이 live root에 생성됨
- `events.ndjson`가 append-only로 계속 증가함
- `state.latest.txt`에 고정 섹션이 들어 있음
- `godot.log`에 manifest mismatch나 assembly load failure가 없음
- `inspect-godot-log` 결과에 `HarmonyPatchAllStartupFailure`가 없음
- `inspect-live-export` 결과에 `DeployedVsLiveRootMismatch`, `RuntimeLogMissing`이 없음

## collector mode일 때 추가 성공 신호

- `raw-observations.ndjson`가 생성됨
- `screen-transitions.ndjson`가 생성됨
- `choice-candidates.ndjson`와 `choice-decisions.ndjson`가 생성됨
- `semantic-snapshots` 폴더에 reward/event/rest/shop 시점 snapshot이 남음
- `codex-trace.ndjson`에 `request-started`, `request-finished`, 필요 시 `request-retried`, `request-coalesced`, `request-superseded`가 남음
- `Analyze Now`는 현재 snapshot 기준 새 요청, `Retry Last`는 마지막 prompt pack 재전송으로 구분되어 기록됨
- `collector-summary.json`에 아래 섹션이 채워짐
  - request latency summary
  - duplicate/coalesced trigger summary
  - screen overwrite summary
  - state regression summary
  - missing information top N
  - decision blockers top N
  - knowledge usage summary
  - runtime fatal errors
  - session tracking status
  - observed merge counts
  - placeholder labels observed
  - auto advice failures
  - apphang suspicion indicators
  - recommended next fixes

## 실패했을 때 먼저 볼 것

- `mods\sts2-mod-ai-companion.runtime.log`
- `%AppData%\SlayTheSpire2\logs\godot.log`
- `artifacts\native-package-layout\flat\native-deploy-report.json`
- collector mode였다면 `artifacts/companion/<run-id>/collector-summary.json`
- 필요 시 최신 snapshot 기준 `restore-snapshot-state`

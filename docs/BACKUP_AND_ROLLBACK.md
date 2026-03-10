# 백업과 롤백

새 모드를 테스트할 때 권장 흐름은 아래와 같습니다.

1. 게임 종료
2. `snapshot`
3. package/deploy
4. 게임 실행 후 smoke test
5. 실패 시 `restore-snapshot-state` 또는 `restore`

기본 백업 대상:

- `release_info.json`
- `settings.save`
- `settings.save.backup`
- `prefs.save`
- `prefs.save.backup`
- `progress.save`
- `progress.save.backup`
- `current_run.save`
- `current_run.save.backup`

CLI 예시:

```powershell
dotnet run --project src/Sts2ModKit.Tool -- snapshot
dotnet run --project src/Sts2ModKit.Tool -- verify-snapshot
dotnet run --project src/Sts2ModKit.Tool -- restore-snapshot-state
```

진행 데이터가 초기화된 것처럼 보이면:

- vanilla는 `profileN`
- modded는 `modded/profileN`

로 분리된 상태일 가능성이 큽니다.

이 경우:

```powershell
dotnet run --project src/Sts2ModKit.Tool -- sync-modded-profile
```

를 사용합니다.

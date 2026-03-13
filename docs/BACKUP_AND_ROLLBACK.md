# 백업과 롤백

게임 `mods` 폴더나 modded profile을 건드리는 live 배포/검증 전에는 반드시 snapshot과 restore 경로를 준비합니다.

## 권장 순서

1. 게임을 종료합니다.
2. `dotnet run --project src\Sts2ModKit.Tool -- snapshot`
3. 모드를 빌드/패키징/배포합니다.
4. live smoke를 수행합니다.
5. 상태가 오염됐으면 `restore-snapshot-state` 또는 `restore`로 되돌립니다.

## snapshot이 포함하는 범위

- `release_info.json`
- user settings 및 backup
- profile save 및 backup
- 현재 run save가 있으면 그 파일
- AI companion이 건드리는 게임 `mods` 폴더의 tracked 파일

## 자주 쓰는 명령

```powershell
dotnet run --project src\Sts2ModKit.Tool -- snapshot
dotnet run --project src\Sts2ModKit.Tool -- verify-snapshot
dotnet run --project src\Sts2ModKit.Tool -- restore-snapshot-state
dotnet run --project src\Sts2ModKit.Tool -- restore --snapshot-root <path>
```

## 주의

- `restore-snapshot-state`는 snapshot 당시 없던 tracked 파일도 삭제합니다.
- `verify-snapshot`은 반복 smoke 전에 상태가 깨졌는지 확인할 때 유용합니다.
- `sync-modded-profile`은 롤백이 아니라 vanilla profile을 modded profile로 복사하는 별도 도구입니다.

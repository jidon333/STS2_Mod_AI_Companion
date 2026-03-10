# Smoke test 체크리스트

새 모드 첫 배포 전에는 아래 순서로 점검합니다.

1. `show-config`로 현재 경로와 template 메타데이터 확인
2. `snapshot` 실행
3. `materialize-native-package` 실행
4. 필요하면 `build-native-pck` 실행
5. `deploy-native-package` 실행
6. 게임 실행
7. 로그 확인
8. save/profile 이상 여부 확인

확인할 로그:

- 게임 로그: mod loader가 `.pck`와 `.dll`을 읽었는지
- 모드 로그: template runtime log가 생성됐는지

최소 성공 기준:

- mod loader가 `.pck`와 matching `.dll`을 인식
- `mod_manifest.json` mismatch 오류 없음
- 게임 부팅 성공
- `modded/profileN` 경로 분리 여부 확인
- rollback 절차가 실제로 동작

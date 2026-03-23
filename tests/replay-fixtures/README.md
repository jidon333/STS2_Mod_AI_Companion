# Replay Fixtures

이 폴더는 harness mode에서 사용하는 재생 입력 묶음을 둡니다.

- `live export` 스냅샷
- collector summary
- advice trace
- failure triage bundle

현재 포함된 대표 묶음:

- `gui-smoke-golden-scenes.json`
  - foreground/background winner와 candidate suppression 회귀를 보는 golden scene suite
- `gui-smoke-parity-scenes.json`
  - saved request와 `--full-request-rebuild` replay가 같은 결론을 내리는지 보는 M6 parity suite
- `m6-parity/`
  - 최신 stable long-run roots에서 잘라낸 request / observer sidecar / screenshot fixture들

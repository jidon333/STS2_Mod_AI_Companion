# AI Handoff Prompt (KO)

## 문서 목적

이 문서는 새 `참모 세션`과 새 `구현+테스트 세션`이 지금 상태를 빠르게 이어받기 위한 hand-off 문서다.

이 문서가 답해야 하는 질문은 네 가지다.

1. 지금 프로젝트의 실제 방향성은 무엇인가
2. 지금까지 무엇을 해결했고 무엇은 아직 안 끝났는가
3. 다음 세션이 바로 손대야 할 가장 중요한 문제는 무엇인가
4. 어떤 검증 루프로 판단해야 false success를 피할 수 있는가

중요:

- 이 문서는 현재 시점의 작업 hand-off다.
- roadmap 전체를 대체하지 않는다.
- authoritative 판정은 항상 runtime artifact 기준이다.

## 현재 추천 역할 구조

현재 권장 세션 구조는 아래 두 축이다.

1. `참모 세션`
   - 문제 분해
   - 우선순위 결정
   - 구현 결과 검토
   - artifact 기준 판정
2. `구현+테스트 통합 세션`
   - 코드 수정
   - build / self-test / replay-test / live run
   - 재현 root 생성
   - 결과 보고

지금 단계에서는 `구현`과 `테스트`를 강하게 분리하기보다, 하나의 구현 세션이 짧은 디버그-수정-재검증 루프를 직접 닫는 편이 더 효율적이다.

## 현재 방향성

### 1. 장기 목표는 read-only advisor 제품이다

- 전체 목표는 여전히 `Phase 1: 외부 프로세스 AI 조언 어시스턴트 완성`이다.
- 최종적으로는 사람이 실제 플레이 중 참고 가능한 `read-only advisor`를 만든다.
- 장기 마일스톤은 [ROADMAP.md](../../ROADMAP.md)의 `M1~M10`을 canonical source로 따른다.

### 2. 현재 critical path는 gameplay가 아니라 startup / trust다

- 최근 몇 차례 문서와 artifact truthfulness 개선으로 `무엇이 보였는가`는 전보다 훨씬 정직하게 읽히게 됐다.
- 하지만 current-execution 기준으로는 여전히:
  - loader entry signal 없음
  - runtime exporter 없음
  - harness bridge 없음
  - fresh snapshot 없음
- 그래서 지금 active milestone은 `M2. 모드 로드 진입 증명`이다.
- `combat`, `event/map overlay`, `reward fallback`은 중요한 기술 부채이지만 현재 top blocker는 아니다.

### 3. 현재 올바른 표현은 "startup truthfulness는 좋아졌지만 root cause는 아직 loader/runtime rail에 있다"이다

- startup timeline, sentinel, delta, reviewer summary는 현재 실행의 progression을 더 정직하게 보여준다.
- 하지만 이건 `왜 안 되는지를 더 잘 보이게 한 것`이지, `이미 작동한다`는 뜻이 아니다.
- 지금 필요한 것은 더 많은 진단 레이어가 아니라 `ModManager.TryLoadModFromPck(...)` 기준 failing branch 직접 디버깅이다.

## 지금까지 실제로 전진한 일

### A. startup artifact truthfulness 정리

해결 결과:

- launch baseline / delta capture가 생겼다
- module-initializer probe가 pure probe로 축소됐다
- startup timeline과 reviewer summary가 생겨 `latest`와 `ever/first-positive`를 분리해서 읽을 수 있다
- stale snapshot과 fresh snapshot, no-snapshot 상태를 구분해서 기록한다

대표 root:

- [verify-startup-sentinel-20260319-193531](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-sentinel-20260319-193531)
- [verify-startup-timeline-20260319-212903](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-timeline-20260319-212903)

### B. session summary / restart chain honesty 보강

해결 결과:

- `session-summary.json`이 live restart를 즉시 반영한다
- invalid attempt를 optimistic success로 세지 않게 됐다
- reviewer가 현재 active attempt와 terminal attempt를 더 일관되게 읽을 수 있다

### C. gameplay safety 쪽 최근 전진

이건 현재 critical path는 아니지만, 이미 닫은 성과로 남는다.

- stale curse/status non-enemy promotion 차단
- enemy-turn closure
- repeated non-enemy stale loop 차단
- slot-4 combat no-op loop 대응
- replay / parity self-test와 spot-check 유지

## 현재 가장 중요한 문제

현재 주 병목은 `current execution에서 primary DLL load-chain이 실제로 어디까지 도달하는가`다.

핵심 root:

- [verify-startup-timeline-20260319-212903](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-timeline-20260319-212903)

핵심 진단:

- [startup-runtime-evidence.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-timeline-20260319-212903/startup-runtime-evidence.json)
  - latest diagnosis = `loader-entry-before-initializer-not-proven`
  - `everReachedMainMenu=true`
  - `everSawStaleSnapshot=true`
  - `everSawCurrentExecutionSentinel=false`
  - `everSawRuntimeExporter=false`
- [startup-runtime-captures.ndjson](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-timeline-20260319-212903/startup-runtime-captures.ndjson)
  - earlier positive와 later negative가 같은 root 안에 공존한다

### 이 문제가 왜 중요한가

- 지금은 gameplay가 막힌 것이 아니라 startup rail에서 authoritative current-execution proof가 멈춘 상태다.
- 이 상태에서 gameplay를 더 파면 false success 위험이 크다.
- 즉 지금은 "옵저버가 약한가"보다 "`loader / runtime bootstrap`이 실제로 current run에서 안 살아나는가"가 더 중요하다.

### 현재 가장 강한 작업 가설

- 더 이상의 startup diagnostic schema 추가는 한계효용이 낮다.
- 다음은 `ModManager.TryLoadModFromPck(...)` contract 기준 direct loader debugging이어야 한다.
- 특히 아래를 직접 대조해야 한다.
  - deployed `sts2-mod-ai-companion.pck`
  - deployed `sts2-mod-ai-companion.dll`
  - embedded `res://mod_manifest.json`
  - `pck_name` / loose DLL naming / package layout
  - current binary identity와 stale payload 가능성

## 지금 당장 해야 할 일

### 1. direct loader debugging only

다음 세션 목표:

- `sts2-mod-ai-companion.dll`이 왜 game mod loader에 실제로 안 올라오는지 failing branch 1개를 특정한다.
- 가능하면 같은 세션에서 그 branch를 수정하고 첫 positive loader/runtime signal 1개를 되살린다.

금지:

- 새 sentinel 계층 추가
- 새 summary / timeline schema 추가
- trust threshold 변경
- gameplay / HandleCombat / observer heuristic 튜닝

### 2. source of truth

가장 먼저 읽을 것:

- [ModManager.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/knowledge/decompiled/MegaCrit/sts2/Core/Modding/ModManager.cs)
- [STS2_NATIVE_LOADER.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/STS2_NATIVE_LOADER.md)
- [mod_manifest.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/native-package-layout/flat/export-project/mod_manifest.json)

### 3. bounded 작업

1. `TryLoadModFromPck` checklist를 branch별로 현재 payload와 대조
2. package / deploy / `.pck` contents / embedded manifest 직접 검증
3. current primary DLL identity와 stale binary 가능성 확인
4. known-good 시점의 payload와 현재 payload 비교 가능하면 수행
5. failing branch를 찾으면 packaging / deploy / runtime bootstrap 범위 안에서 바로 수정

## 현재 읽어야 할 가장 중요한 artifact

### 현재 startup blocker 이해용

- [verify-startup-sentinel-20260319-193531](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-sentinel-20260319-193531)
- [verify-startup-timeline-20260319-212903](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-timeline-20260319-212903)
- [startup-runtime-evidence.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-timeline-20260319-212903/startup-runtime-evidence.json)
- [startup-runtime-captures.ndjson](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-timeline-20260319-212903/startup-runtime-captures.ndjson)
- [prevalidation.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-timeline-20260319-212903/prevalidation.json)
- [supervisor-state.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-timeline-20260319-212903/supervisor-state.json)

### loader contract 이해용

- [ModManager.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/knowledge/decompiled/MegaCrit/sts2/Core/Modding/ModManager.cs)
- [mod_manifest.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/native-package-layout/flat/export-project/mod_manifest.json)
- [NativeModPackaging.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2ModAiCompanion.Mod/NativeModPackaging.cs)

## 중요한 guardrail

1. startup / deploy / trust가 invalid면 gameplay debugging을 하지 않는다
2. 하네스가 게임 내부 선택 API를 직접 호출하게 만들지 않는다
3. startup diagnostic layer를 계속 추가하지 않는다
4. direct loader debugging은 decompiled contract + deployed payload + fresh artifact로 닫는다
5. 세션 결과는 항상 artifact 기준으로만 판정한다

## 새 세션 권장 시작 루프

### 참모 세션

1. latest startup root 읽기
2. current authoritative blocker를 한 줄로 압축
3. 구현 범위를 `loader/package/deploy/runtime bootstrap` 안으로 고정
4. 구현 세션 프롬프트 승인

### 구현+테스트 세션

1. `dotnet build`
2. `dotnet run --project src/Sts2GuiSmokeHarness -- self-test`
3. payload / `.pck` / manifest / DLL identity branch 점검
4. 필요 시 packaging/deploy 수정
5. fresh startup-focused live run 1회
6. startup artifact 기준으로 loader positive signal 여부 판정

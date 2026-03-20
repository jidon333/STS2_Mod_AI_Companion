# 로드 체인

> Status: Reference
> Source of truth: No
> Use this for end-to-end background, not for current blocker or latest milestone pointer.

이 문서는 `게임 실행 -> 모드 로드 -> exporter 동작 -> 외부 앱 표시`까지의 전체 체인을 정리합니다.

## 1. 게임 시작

권장 실행 방식은 direct exe가 아니라 Steam URI입니다.

```powershell
cmd /c start "" "steam://rungameid/2868840"
```

이 경로를 쓰는 이유는 Steam app id 초기화와 런처 환경이 더 안정적이기 때문입니다.

## 2. STS2 로더가 `mods` 폴더를 스캔

게임 설치 폴더 아래의 `mods` 폴더를 스캔해서 `.pck`와 같은 basename의 `.dll`을 찾습니다.

현재 AI Companion 패키지는 아래 구성으로 배포됩니다.

- `sts2-mod-ai-companion.pck`
- `sts2-mod-ai-companion.dll`
- `Sts2ModKit.Core.dll`
- `sts2-mod-ai-companion.config.json`

## 3. `.dll` 로드 후 Harmony 초기화

모드 payload가 로드되면 runtime config를 읽고 Harmony 훅을 적용합니다.

이 단계의 주요 진단 로그:

- `%AppData%\SlayTheSpire2\logs\godot.log`
- 게임 `mods` 폴더의 `sts2-mod-ai-companion.runtime.log`

## 4. exporter 초기화

exporter는 아래를 준비합니다.

- 런타임 observation 수집
- canonical snapshot 유지
- writer queue
- live output root 생성

writer queue를 두는 이유는 게임 메서드 안에서 직접 파일 I/O를 하지 않기 위해서입니다.

## 5. live export 파일 기록

exporter가 정상 동작하면 아래 파일이 생깁니다.

- `events.ndjson`
- `state.latest.json`
- `state.latest.txt`
- `session.json`

`events.ndjson`는 append-only이고, 나머지 3개는 latest overwrite입니다.

## 6. static knowledge pipeline

개발자 툴은 별도로 게임 DLL과 PCK에서 정적 지식을 뽑아냅니다.

현재 단계:

- `release-scan`
- `decompile-scan`
- `assembly-scan`
- `pck-inventory`
- `strict-domain-parse`
- `localization-scan`
- `observed-merge`
- `catalog-build`

주요 산출물:

- `catalog.latest.json`
- `catalog.latest.txt`
- `source-manifest.json`
- `decompile-scan.json`
- `strict-domain-scan.json`
- `assembly-scan.json`
- `pck-inventory.json`
- `localization-scan.json`
- `observed-merge.json`
- `catalog.assistant.json`
- `assistant/*.json`
- `markdown/*.md`

## 7. 외부 Host가 live export를 감시

`Sts2AiCompanion.Host`는 현재 live export를 polling으로 읽습니다.

Host가 하는 일:

- run 경계 감지
- 최근 이벤트 tail 유지
- knowledge slice 선택
- prompt pack 생성
- Codex CLI 실행
- advice artifact 저장
- same run 기준 `codex-session.json` 세션 복구
- high-priority automatic trigger latest-only coalescing

## 8. WPF 앱이 최종 사용자 표면

`Sts2AiCompanion.Wpf`는 Host를 같은 프로세스에서 띄우고, 아래를 표시합니다.

- 현재 상태
- 최근 선택지
- 최근 이벤트
- 관련 지식
- 최신 AI 조언

즉, 사용자는 게임 안이 아니라 별도 창에서 조언을 보게 됩니다.

추가로 WPF는 아래 동작도 제공합니다.

- `Analyze Now`: 현재 snapshot 기준 새 prompt 생성
- `Retry Last`: 마지막 prompt pack 재전송
- 모델 / 추론 강도 선택
- collector 진단 표시

## 9. 실패 시 어디를 봐야 하나

1. `godot.log`
   - loader / Harmony startup failure
2. `sts2-mod-ai-companion.runtime.log`
   - runtime identity / hook summary
3. `inspect-live-export`
   - live root, runtime config, runtime log, artifact 4종 상태
4. `artifacts/knowledge`
   - knowledge pipeline 산출물
5. `artifacts/companion`
   - prompt pack, `advice/`, `codex-session.json`, `codex-trace.ndjson`, collector summary

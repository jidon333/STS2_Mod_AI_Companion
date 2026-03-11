# 아키텍처

## 1. 게임 내부: native STS2 모드

게임 프로세스 안에 로드되는 작은 shell입니다.

역할:

- loader 호환성 유지
- runtime config 로드
- Harmony 훅 등록
- read-only 상태 관찰

## 2. runtime exporter

게임 안에서 관찰한 상태를 아래 파일로 내보냅니다.

- `events.ndjson`
- `state.latest.json`
- `state.latest.txt`
- `session.json`

핵심 원칙:

- 게임 상태를 바꾸지 않음
- 파일 쓰기는 background writer queue에서만 처리
- latest 파일은 atomic overwrite

## 3. static knowledge pipeline

게임 외부에서는 정적 지식도 별도로 추출합니다.

주요 단계:

- `release-scan`
- `assembly-scan`
- `pck-inventory`
- `localization-scan`
- `observed-merge`
- `catalog-build`

산출물은 `artifacts/knowledge` 아래에 저장됩니다.

중요:

- 사람이 읽는 markdown 리포트와
- AI가 직접 읽는 `catalog.assistant.*`, `assistant/*.json`

을 따로 생성합니다.

## 4. 외부 host

`Sts2AiCompanion.Host`는 live export와 knowledge catalog를 묶어 Codex 요청 단위로 정리하는 레이어입니다.

주요 역할:

- live export polling
- run 경계 추적
- knowledge slice 선택
- prompt pack 생성
- Codex CLI 호출
- advice artifact 저장

## 5. 외부 WPF 앱

`Sts2AiCompanion.Wpf`는 최종 사용자 표면입니다.

보여 주는 것:

- 현재 상태
- 현재 화면
- 선택지
- 최근 이벤트
- 관련 knowledge slice
- 최신 AI 조언

## 6. 안전 경계

- snapshot / restore 경로 유지
- direct exe 대신 Steam URI 사용
- read-only exporter 유지
- 게임이 죽지 않아도 외부 앱은 죽을 수 있어야 함
- 외부 앱이 죽어도 게임은 계속 진행 가능해야 함

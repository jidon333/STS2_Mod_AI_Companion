# Spire Codex 참고 메모

> Status: Reference
> Source of truth: No
> Use this as an external reference note, not as a current architecture or contract doc.

이 문서는 `https://github.com/ptrlrd/spire-codex`와 `https://spire-codex.com/`에서 무엇을 참고했고, 현재 `STS2_Mod_AI_Companion`에 무엇을 반영했는지 정리합니다.

원칙:

- `Spire Codex`는 방법론 참고와 교차검증 힌트로만 사용합니다.
- 현재 저장소의 canonical source는 계속 로컬 게임 설치본과 런타임 관찰 결과입니다.
- 외부 사이트 데이터를 복사해 catalog를 채우지 않습니다.

## 1. 확인한 핵심 방법론

`Spire Codex` 공개 파서 구조에서 얻은 핵심은 아래입니다.

1. `sts2.dll`을 디컴파일한 실제 `Models/*` 소스를 기준으로 도메인을 분류합니다.
2. `SlayTheSpire2.pck`에서 localization과 리소스 경로를 복구합니다.
3. 카드/유물/포션/이벤트는 도메인별 전용 parser로 읽습니다.
4. 설명 본문은 localization과 구조 정보를 함께 써서 해석합니다.
5. 이벤트는 단일 텍스트가 아니라 `pages + options` 구조로 다룹니다.

즉 `Card`라는 문자열이 들어간다고 카드를 분류하는 방식이 아니라, 실제 모델 클래스와 도메인별 localization을 기준으로 canonical 엔트리를 만드는 쪽입니다.

## 2. 현재 저장소에 반영한 내용

### 2.1 decompile-scan 도입

현재 저장소도 `sts2.dll` 디컴파일을 knowledge pipeline의 정식 단계로 넣었습니다.

현재 파이프라인:

1. `release-scan`
2. `decompile-scan`
3. `assembly-scan`
4. `pck-inventory`
5. `strict-domain-parse`
6. `localization-scan`
7. `observed-merge`
8. `catalog-build`

### 2.2 strict parser 도입

현재 저장소는 아래 도메인에서 substring match 대신 strict parser를 씁니다.

- cards
- relics
- potions
- events
- shops
- rewards
- keywords

기준:

- 디컴파일된 `MegaCrit.Sts2.Core.Models.*` 소스
- card/relic pool 소스
- domain-specific localization key stem

그래서 `OPTION_*`, `*_BUTTON`, `*_POWER` 같은 잡음이 cards canonical에 남지 않게 됐습니다.

### 2.3 이벤트를 `pages + options` 구조로 유지

`Spire Codex`의 이벤트 파서 방향을 참고해, 현재 저장소도 아래를 유지합니다.

- 이벤트 title
- 이벤트 description
- page key들
- option key들

현재 assistant export도 이벤트 옵션 배열을 유지합니다.

### 2.4 assistant export와 사람용 리포트 분리

`Spire Codex`가 API/프론트엔드용 구조를 따로 정리하듯, 현재 저장소도 사람이 읽는 markdown와 AI가 읽는 assistant export를 분리했습니다.

사람용:

- `artifacts/knowledge/markdown/*.md`

AI용:

- `artifacts/knowledge/catalog.assistant.json`
- `artifacts/knowledge/catalog.assistant.txt`
- `artifacts/knowledge/assistant/*.json`
- `artifacts/knowledge/assistant/index.json`

## 3. 현재 저장소가 의도적으로 하지 않은 것

아래는 참고는 했지만 그대로 가져오지 않은 부분입니다.

- 사이트/API 데이터를 직접 가져와 catalog에 반영
- `Spire Codex`의 데이터를 source of truth로 취급
- 외부 사이트 설명을 그대로 복사

현재 truth source:

- 로컬 `sts2.dll`
- 로컬 `SlayTheSpire2.pck`
- 로컬 `release_info.json`
- 실제 런타임 `live export`

## 4. 현재 남아 있는 차이

`Spire Codex`는 대형 데이터 백과사전 프로젝트이고, 현재 저장소는 실시간 조언 어시스턴트 프로젝트입니다.

그래서 아직 차이가 있습니다.

- `shops/rewards/keywords`도 이제 strict semantic parser가 있지만, `shops/rewards`는 실제 런의 상품/보상 배치를 뜻하는 것이 아니라 시스템 의미 단위를 정리한 값입니다.
- SmartFormat/description resolver는 현재 단순 버전입니다.
- gameplay observed merge는 exporter coverage가 넓어져야 의미 있게 채워집니다.

## 5. 현재 결론

`Spire Codex`에서 얻은 가장 중요한 교훈은 “STS2 데이터는 모델 클래스, localization, 리소스 경로, 런타임 관찰을 함께 봐야 제대로 읽힌다”는 점입니다.

현재 저장소는 그 방법론을 아래처럼 반영한 상태입니다.

- 로컬 게임 파일에서 직접 추출
- 디컴파일된 모델 기준 strict parser 적용
- localization 본문을 canonical 엔트리에 병합
- 이벤트 옵션 구조 유지
- assistant export를 별도 생성
- 외부 사이트는 교차검증 힌트로만 사용

# Spire Codex 참고 메모

이 문서는 `https://github.com/ptrlrd/spire-codex`와 `https://spire-codex.com/`을 확인해서 무엇을 알게 되었고, 현재 `STS2_Mod_AI_Companion` 저장소에 무엇을 반영했고 무엇은 반영하지 않았는지를 정리한 문서입니다.

중요한 원칙은 아래와 같습니다.

- `Spire Codex`는 설계 참고와 교차검증 힌트로만 사용합니다.
- 현재 저장소의 canonical source는 계속 로컬 게임 설치본과 런타임 관찰 결과입니다.
- 외부 사이트의 데이터를 복사해 catalog를 채우지 않습니다.

## 1. 원격 저장소에서 확인한 사실

`Spire Codex` README와 공개 파서 구조를 확인한 결과, 이 프로젝트는 아래 순서로 데이터를 만듭니다.

1. `SlayTheSpire2.pck`를 추출해 localization, 이미지, Spine 자산을 복구합니다.
2. `sts2.dll`을 디컴파일해 읽을 수 있는 C# 소스로 만듭니다.
3. 파서 여러 개를 돌려 카드, 유물, 포션, 이벤트, 파워, 키워드, 조우, 고대인, 타임라인 등의 구조화된 JSON을 만듭니다.
4. `description_resolver.py`로 SmartFormat 기반 설명 문장을 사람이 읽을 수 있는 형태로 풉니다.
5. 프론트엔드와 API를 통해 그 결과를 데이터베이스/브라우저 형태로 노출합니다.

README와 공개 파일 목록 기준으로 특히 눈에 띄는 포인트는 아래입니다.

- 카드/유물/포션/이벤트/키워드/조우/파워를 도메인별로 강하게 분리합니다.
- 카드 정보는 단순 이름이 아니라 비용, 타입, 희귀도, 타겟, 업그레이드 정보까지 노립니다.
- 이벤트는 단일 텍스트가 아니라 `multi-page + options` 구조로 파싱합니다.
- 설명 본문은 localization 원문과 코드상의 수치/변수 정보를 함께 사용해 복원합니다.
- `description_resolver.py`에서 `{Damage:diff()}`, `{Energy:energyIcons()}`, `{Cards:plural:card|cards}` 같은 SmartFormat 토큰을 해석합니다.
- 이벤트 파서는 `StringVar`와 localization을 결합해 이벤트 선택지와 문장 안의 동적 이름을 더 읽기 좋게 바꿉니다.

## 2. 현재 저장소에서 실제로 반영한 점

현재 저장소는 `Spire Codex`의 구현을 가져온 것이 아니라, 아래와 같은 방향성만 참고했습니다.

### 2.1 도메인 분리 방식

`Spire Codex`처럼 카드/유물/포션/이벤트/상점/보상/키워드를 분리해서 다루는 방식이 유용하다고 판단했습니다.

그래서 현재 산출물도 아래 top-level 섹션을 기준으로 유지합니다.

- `cards`
- `relics`
- `potions`
- `events`
- `shops`
- `rewards`
- `keywords`
- `metadata`

관련 코드:

- `src/Sts2ModKit.Core/Knowledge/StaticKnowledgeCatalogBuilder.cs`
- `src/Sts2ModKit.Core/Knowledge/AssistantKnowledgeExport.cs`

### 2.2 localization을 중심에 두는 방식

`Spire Codex` README와 `description_resolver.py`를 보면, 게임 설명 본문은 localization과 구조 정보의 결합으로 읽어야 한다는 점이 분명합니다.

이 아이디어를 받아 현재 저장소에서도 `localization-scan` 단계를 정식 파이프라인으로 추가했습니다.

현재 파이프라인:

1. `release-scan`
2. `assembly-scan`
3. `pck-inventory`
4. `localization-scan`
5. `observed-merge`
6. `catalog-build`

관련 코드:

- `src/Sts2ModKit.Core/Knowledge/LocalizationKnowledgeScanner.cs`
- `src/Sts2ModKit.Tool/StaticKnowledgeCommands.cs`

### 2.3 이벤트를 `페이지 + 선택지`로 보는 관점

`Spire Codex`의 `event_parser.py`는 이벤트를 단순 설명 하나로 보지 않고, 페이지와 옵션 구조로 읽습니다.

이 점은 현재 저장소에도 반영되어 있습니다.

- 이벤트 localization에서 `pages.*.description`을 읽습니다.
- 이벤트 선택지는 `pages.*.options.*.title/description`까지 수집합니다.
- assistant export에서도 이벤트 옵션 배열을 유지합니다.

이 덕분에 이후 실제 gameplay에서 관찰한 선택지와 offline 지식 카탈로그를 더 잘 연결할 수 있습니다.

### 2.4 AI용 별도 export 필요성

`Spire Codex`가 API/프론트엔드용 구조를 따로 정리한다는 점을 참고해, 현재 저장소도 사람이 읽는 markdown와 AI가 읽는 assistant export를 분리했습니다.

현재 AI가 직접 읽는 쪽:

- `artifacts/knowledge/catalog.assistant.json`
- `artifacts/knowledge/catalog.assistant.txt`
- `artifacts/knowledge/assistant/*.json`
- `artifacts/knowledge/assistant/index.json`

현재 사람용 문서:

- `artifacts/knowledge/markdown/*.md`

## 3. 현재 저장소가 의도적으로 하지 않은 것

아래는 참고는 했지만 그대로 가져오지 않은 부분입니다.

### 3.1 외부 데이터를 그대로 가져오지 않음

현재 저장소는 `Spire Codex`의 API나 JSON 데이터를 직접 가져다 쓰지 않습니다.

하지 않는 것:

- 사이트 데이터 스크래핑
- API 응답을 그대로 catalog에 반영
- README의 수치나 설명을 그대로 복사
- 외부 사이트를 source of truth로 취급

현재 저장소의 truth source:

- 로컬 `sts2.dll`
- 로컬 `SlayTheSpire2.pck`
- 로컬 `release_info.json`
- 실제 런타임 `live export`

### 3.2 전체 PCK 정식 언팩/복원 파이프라인은 아직 없음

`Spire Codex`는 GDRE, ILSpy, Python 파서, Node Spine renderer까지 포함한 대형 데이터 추출 프로젝트입니다.

현재 저장소는 그 전체를 재현하지 않습니다.

현재 범위:

- 런타임 exporter
- 로컬 정적 지식 추출
- L10N 스캔
- assistant용 knowledge export
- WPF 조언 어시스턴트 준비

즉 `게임 데이터 백과사전 사이트`를 만드는 것이 아니라, `실시간 조언 어시스턴트`를 만드는 쪽이 중심입니다.

## 4. 지금 실제로 얻은 힌트

이번 참조로부터 현재 저장소에 실질적으로 남은 힌트는 아래와 같습니다.

1. localization 설명을 구조 정보와 분리하지 말고 함께 다뤄야 합니다.
2. 이벤트는 제목 하나보다 `페이지 + 옵션 + 동적 변수`가 중요합니다.
3. AI 입력용 구조와 사람용 문서는 분리하는 편이 낫습니다.
4. 카드/유물/포션/이벤트/키워드는 같은 catalog 안에서도 도메인별 처리 전략이 달라야 합니다.
5. 외부 사이트는 검산용으로 유용하지만 canonical source로 쓰면 안 됩니다.

## 5. 현재 저장소에서 교차검증에 쓰는 방식

현재 저장소는 `spire-codex.com`을 직접 데이터 소스로 쓰지 않고, 아래처럼 교차검증 힌트로만 사용합니다.

- 어떤 도메인이 빠져 있는지 점검
- 이벤트/옵션 구조가 지나치게 납작하지 않은지 점검
- 카드/유물/포션 설명 본문이 비어 있지 않은지 점검
- assistant export에 필요한 필드가 너무 빈약하지 않은지 점검

관련 메타데이터:

- `artifacts/knowledge/assistant/index.json`
  - `externalCrossCheckHints`
  - `primarySources`
  - `metadata`
  - `provenance`

## 6. 현재 결론

`Spire Codex`에서 얻은 가장 중요한 교훈은 “STS2 데이터는 localization, 코드 구조, 리소스 경로, 런타임 관찰을 함께 봐야 제대로 읽힌다”는 점입니다.

현재 저장소는 그 결론을 받아들여 다음처럼 정리되어 있습니다.

- 로컬 게임 파일에서 직접 추출
- localization 본문을 catalog에 병합
- 이벤트 옵션과 선택지를 구조화
- AI용 export를 별도로 생성
- 외부 사이트는 교차검증 힌트로만 사용

즉 현재 저장소는 `Spire Codex를 복제`하는 것이 아니라, `Spire Codex가 보여준 데이터 해석 방향`을 현재 프로젝트 목적에 맞게 축소·적용한 상태입니다.

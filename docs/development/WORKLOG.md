# 작업 기록

이 문서는 저장소가 어떤 순서로 지금 상태까지 왔는지 압축해서 기록합니다. 오래된 실험과 현재 상태를 구분하기 위해, “당시 사실”과 “현재 기준”이 다를 수 있는 항목은 그대로 시간순으로 남깁니다.

## 1. 저장소 정리

- 로컬 멀티 에이전트 오케스트레이션 레이어를 제거했습니다.
- 저장소 중심을 다시 `모드 구현 + exporter + packaging + snapshot/restore + live smoke + knowledge extraction`으로 맞췄습니다.
- 상위 문서와 개발 문서 체계를 정리했습니다.

## 2. runtime exporter 기초 구축

- live export 4종을 기록하는 경로를 만들었습니다.
  - `events.ndjson`
  - `state.latest.json`
  - `state.latest.txt`
  - `session.json`
- writer queue, state tracker, summary formatter, replay/self-test 기반을 붙였습니다.

## 3. Harmony startup failure 정리

- broad generic hook와 void postfix 문제 때문에 발생하던 `PatchAll` startup failure를 정리했습니다.
- startup identity와 hook summary 로그를 남기게 했습니다.
- main menu까지 live export가 다시 생성되는 상태를 확인했습니다.

## 4. 정적 지식 파이프라인 1차 도입

- observed-only 경로에서 벗어나 `release-scan`, `assembly-scan`, `pck-inventory`, `observed-merge`, `catalog-build` 파이프라인을 붙였습니다.
- `catalog.latest.json`, `catalog.latest.txt`, `source-manifest.json`을 canonical 산출물로 고정했습니다.
- `assembly-scan.json`, `pck-inventory.json`, `observed-merge.json` 같은 중간 산출물도 보존하게 했습니다.

## 5. Host / WPF 뼈대 추가

- `Sts2AiCompanion.Host` 프로젝트를 추가했습니다.
- `Sts2AiCompanion.Wpf` 프로젝트를 추가했습니다.
- live export polling, knowledge slice, prompt pack, Codex CLI 연동, run artifact 저장 계약을 만들었습니다.

## 6. 사람이 읽는 markdown 리포트 정리

- `artifacts/knowledge/markdown` 아래에 카드/유물/포션/이벤트/상점/보상/키워드 문서를 생성하도록 정리했습니다.
- `PLAY_GUIDE.md`를 추가해, 정적 분석 결과가 실제 플레이에서 어디에 쓰이는지 설명하게 했습니다.

## 7. localization-scan 도입

- `SlayTheSpire2.pck`에서 localization key/value를 직접 복구하는 `localization-scan` 단계를 추가했습니다.
- 카드 seed와 localization을 병합해 title/description/selection prompt를 canonical entry에 붙였습니다.
- 이후 유물, 포션, 이벤트, 상점, 보상, 키워드까지 L10N 연결 범위를 넓혔습니다.

## 8. assistant export 분리

- 사람이 읽는 markdown과 AI가 읽는 assistant export를 분리했습니다.
- `catalog.assistant.json`, `catalog.assistant.txt`, `assistant/*.json`, `assistant/index.json`을 생성하도록 정리했습니다.
- assistant provenance에 primary source, cross-check hint, source chain을 기록하게 했습니다.

## 9. 문서 체계 보강

- `docs/development` 아래에 초보자용 설명, 로드 체인, 구조 문서, AI 아키텍처, WPF 사용자 흐름, Spire Codex 참고 메모를 정리했습니다.
- README와 상위 아키텍처 문서를 개발/검증 절차 중심으로 다시 썼습니다.

## 10. Spire Codex 방법론 반영

- `spire-codex`를 참고해 substring match 기반 분류가 아니라 `모델 클래스 기반 strict parser`로 가야 한다는 방향을 확정했습니다.
- `Spire Codex`의 구현을 복사하지는 않고, 아래 방법론만 반영했습니다.
  - 디컴파일된 `Models/*` 디렉터리 기준 도메인 분류
  - localization과 구조 정보 결합
  - 이벤트를 `pages + options` 구조로 유지
  - assistant export와 사람용 문서 분리

## 11. decompile-scan + strict-domain-parse 도입

- `ilspycmd`를 사용해 `sts2.dll`을 디컴파일하는 `decompile-scan` 단계를 추가했습니다.
- `StrictDomainKnowledgeScanner`를 추가해 아래 도메인을 실제 모델 클래스 기준으로 정규화했습니다.
  - cards
  - relics
  - potions
  - events
- 이 단계 이후 cards canonical에서 `OPTION_*`, `*_BUTTON`, `*_POWER` 같은 잡음이 제거됐습니다.
- strict parser 결과를 canonical seed로 쓰고, broad assembly/pck seed는 raw/intermediate로만 유지하도록 구조를 바꿨습니다.

## 12. localization merge 정교화

- strict seed가 없는 항목은 cards/relics/potions/events canonical에 새 엔트리로 만들지 않게 바꿨습니다.
- description merge 시 `descriptionRaw`, `englishDescriptionRaw`, `flavorRaw` 등을 함께 보존하게 했습니다.
- 단순 placeholder resolver를 도입해 일부 카드/유물 설명에서 동적 변수 치환을 시도하게 했습니다.

## 13. 현재 단계의 실제 결과

현재 최신 `inspect-static-knowledge` 기준:

- strict canonical counts
  - cards: `576`
  - relics: `288`
  - potions: `63`
  - events: `58`
- localization coverage
  - cards: `575` / descriptions: `562` / selection prompts: `22`
  - relics: `285`
  - potions: `75`
  - events: `153` / options: `203`

즉 현재는 “널널한 카드 후보 인벤토리”가 아니라, `실제 카드 모델과 L10N이 결합된 stricter catalog`가 만들어진 상태입니다.

## 14. 다음 단계

1. `shops/rewards/keywords` strict 정규화
2. SmartFormat/description resolver 보강
3. high-value gameplay smoke 확대
4. `currentChoices`와 canonical catalog의 gameplay 교차검증
5. Host/WPF/Codex end-to-end gameplay 검증

## 15. shops / rewards / keywords strict semantic 정규화 완료

- `StrictDomainKnowledgeScanner`를 확장해 `shops`, `rewards`, `keywords`도 실제 디컴파일된 모델/의미 단위 기준으로 정규화했습니다.
- `shops`는 `상점`, `카드 제거 서비스`, `카드/포션/유물 판매 슬롯` 5개 의미 단위만 남겼습니다.
- `rewards`는 `카드/골드/포션/유물/카드 제거/특수 카드/연결 보상 세트` 7개 의미 단위만 남겼습니다.
- `keywords`는 파워, 의도, 카드 키워드를 strict semantic entry로 재구성했습니다.
- `LocalizationKnowledgeScanner`, `StaticKnowledgeCatalogBuilder`, `StaticKnowledgeFormatting`, `AssistantKnowledgeExport`도 함께 조정해 broad raw key가 canonical/assistant/markdown에 다시 들어오지 않게 막았습니다.

최신 `inspect-static-knowledge` 기준:

- cards: `576`
- relics: `288`
- potions: `63`
- events: `58`
- shops: `5`
- rewards: `7`
- keywords: `262`

최신 localization coverage 기준:

- cards: `582` / descriptions: `569` / selection prompts: `22`
- relics: `285` / descriptions: `285`
- potions: `75` / descriptions: `70`
- events: `153` / descriptions: `60` / options: `203`
- shops: `4` / descriptions: `4`
- rewards: `3` / descriptions: `2`
- keywords: `245` / descriptions: `245`

이 단계 이후 `shops/rewards/keywords`의 남은 과제는 노이즈 제거가 아니라, 실제 gameplay 관찰값과의 교차 검증 및 coverage 확대입니다.

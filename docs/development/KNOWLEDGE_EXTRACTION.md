# 정적 지식 추출

이 문서는 카드, 유물, 포션, 이벤트, 상점, 보상 지식을 어떻게 모으는지 설명합니다.

## 1. 왜 정적 지식이 필요한가

실시간 상태만으로는 현재 선택지가 무엇인지는 알 수 있어도, 그 선택지가 가진 맥락을 충분히 설명하기 어렵습니다.

예:

- 카드 이름은 알지만 효과 텍스트를 모름
- 이벤트 선택지가 보이지만 배경 맥락이 약함
- 상점 화면이 떠도 관련 pool이나 이름 정규화가 부족함

그래서 AI 조언 품질을 높이려면 정적 지식 카탈로그가 필요합니다.

## 2. 현재 파이프라인

현재 `extract-static-knowledge`는 아래 단계로 동작합니다.

### 1. release-scan

- `release_info.json`
- Godot log atlas 정보
- 현재 live artifact 존재 여부

를 읽어 메타데이터를 만듭니다.

### 2. assembly-scan

- `sts2.dll`
- 인접 managed DLL

을 `MetadataLoadContext`로 읽어서 타입, enum, 식별자 seed를 수집합니다.

핵심 키워드:

- `Card`
- `Relic`
- `Potion`
- `Event`
- `Shop`
- `Reward`
- `Keyword`
- `Screen`

### 3. pck-inventory

`SlayTheSpire2.pck` 안에서 resource path 후보를 인벤토리화합니다.

현재 구현은 Godot exe가 없어도 기본 문자열 스캔으로 path 후보를 최대한 모으는 공격적 경로를 택합니다.

### 4. observed-merge

live export에서 실제로 본 정보를 병합합니다.

예:

- 덱 카드
- 유물
- 포션
- choice label
- 최근 reward/event/shop 관찰

### 5. catalog-build

위 결과를 합쳐 아래 canonical 산출물을 만듭니다.

- `catalog.latest.json`
- `catalog.latest.txt`
- `source-manifest.json`

## 3. intermediate artifact

중간 산출물도 같이 남깁니다.

- `assembly-scan.json`
- `pck-inventory.json`
- `observed-merge.json`

이 파일들은 정규화 품질을 높이거나 false positive를 줄일 때 유용합니다.

## 4. 현재 한계

- assembly-scan과 pck-inventory는 아직 노이즈가 많습니다.
- 실제 카드/유물 본문 텍스트와 내부 리소스 조각이 섞일 수 있습니다.
- observed-merge는 gameplay coverage가 넓어질수록 더 유용해집니다.

즉, 지금 단계의 정적 카탈로그는 `완성 데이터베이스`가 아니라 `지식 후보 인벤토리 + 정규화 seed`에 가깝습니다.

## 5. 앞으로의 개선 방향

- canonical id 정규화
- false positive 제거
- 이벤트/상점 option 구조 강화
- PCK inventory를 실제 resource graph와 더 잘 연결
- live observation과 offline inventory를 더 강하게 매칭

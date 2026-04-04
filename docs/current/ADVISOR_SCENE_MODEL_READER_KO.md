# Advisor Scene Model 쉬운 설명

> 상태: 현재 사용 중
> 대상 독자: 한국어 사용자, 구현 세션, 리뷰 세션
> 기준 문서:
> - [ADVISOR_SCENE_INFORMATION_MODEL.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/contracts/ADVISOR_SCENE_INFORMATION_MODEL.md)
> - [ADVISOR_UI_COVERAGE_MATRIX_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/ADVISOR_UI_COVERAGE_MATRIX_KO.md)

## 이 문서의 목적

이 문서는 `replay-advisor-scene`이 만들어 내는 결과를 사람이 빨리 읽기 위한 안내서다.

핵심은 하나다.

- raw observer dump를 읽는 문서가 아니다
- AI 추천 결과를 읽는 문서도 아니다
- `현재 장면을 사람이 이해할 수 있는 형태로 정리한 scene model`을 읽는 문서다

## 한 줄로 말하면

scene model은 아래 질문에 답하기 위해 만든다.

```text
지금 화면이 무엇이고,
사람이 실제로 볼 수 있는 선택지는 무엇이며,
아직 모르는 정보는 무엇인가?
```

## 어디서 보나

CLI:

```bash
"/mnt/c/Program Files/dotnet/dotnet.exe" run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-advisor-scene --request <step.request.json>
```

이 명령은 `*.advisor.scene.json`에 해당하는 구조를 stdout으로 출력한다.

Live sidecar:

- WPF 오른쪽 `현재 scene model` 패널
- `artifacts/companion/<runId>/advisor-scene/advisor-scene.latest.json`
- `artifacts/companion/<runId>/advisor-scene/advisor-scene.ndjson`

중요:

- replay와 live는 같은 schema를 쓴다
- 차이는 `sourceKind`
  - replay: `sourceKind=replay`
  - live: `sourceKind=live`
- live에서는 `attemptId`, `stepIndex`, `phase`, `requestPath`, `screenshotPath`가 null일 수 있다

## 어떻게 읽나

### 1. sceneType

현재 장면의 큰 종류다.

예:

- `combat`
- `reward`
- `event`
- `rest-site`
- `shop`
- `map`

### 2. sceneStage

같은 장면 안에서도 지금 어느 단계인지를 뜻한다.

예:

- reward: `claim`
- event: `ancient-option`
- rest-site: `release-pending`
- map: `map-overlay`

이 값은 “화면 이름”보다 더 중요하다.  
advisor는 장면 종류만이 아니라, 그 장면이 지금 어떤 하위 단계인지까지 알아야 한다.

### 3. canonicalOwner

현재 foreground owner가 누구인지 뜻한다.

이 값은 “지금 누구 차례인가”에 가깝다.

예:

- `reward`
- `event`
- `rest-site`
- `map`
- `combat`

중요:

- `visible/open`과 `owner`는 다를 수 있다
- scene model은 현재 하네스의 canonical state를 우선해서 owner를 정한다

### 4. summaryText

사람이 가장 먼저 읽는 요약이다.

예:

- `이벤트 장면(ancient-option). 표시 옵션 3개, 식별된 이벤트 ancient-event.`
- `보상 장면(claim). 보상 항목 3개, proceed 표면이 보인다, 카드 선택 하위 표면이 있다.`
- `지도 장면(map-overlay). 도달 가능 노드 3개, 노드 Monster (1,0), Monster (1,3), Monster (1,6). 누락 정보: map-route-context-missing, map-current-node-identity-missing.`

중요:

- 이 요약에는 actuator vocabulary를 넣지 않는다
- 즉 `allowedActions`, `fallback reason`, `targetLabel` 같은 하네스 내부 용어는 의도적으로 제외한다

WPF에서는 이 값이 `요약` 패널로 그대로 보인다.

### 5. options

지금 사람이 실제로 볼 수 있는 선택지를 정리한 목록이다.

이게 가장 중요한 payload다.

예:

- event에서는 버튼 3개
- reward에서는 보상 항목 + skip/proceed 관련 control choice
- map에서는 갈 수 있는 노드 목록
- rest-site에서는 휴식/재련

중요:

- raw choice dump를 그대로 복사하지 않는다
- 사람이 읽는 선택지 단위로 정리한다

### 6. missingFacts / observerGaps

이 부분은 “추측하지 않고 비워 둔 정보”를 뜻한다.

예:

- `combat-enemy-intent-summary-missing`
- `shop-item-price-missing`
- `shop-item-effect-summary-missing`
- `map-route-context-missing`
- `map-current-node-identity-missing`

읽는 방법:

- `missingFacts`는 사람이나 AI가 지금 판단에 쓰고 싶지만 아직 모델에 없는 정보
- `observerGaps`는 왜 그 정보가 없는지에 대한 시스템 쪽 설명

즉:

```text
missingFacts = 무엇이 부족한가
observerGaps = 왜 부족한가
```

WPF에서는 이 값이 `누락 정보 / observer gaps` 패널로 분리되어 보인다.

### 7. confidence / sourceRefs

- `confidence`는 각 fact band의 신뢰도다
- `sourceRefs`는 현재 scene model이 어떤 seam에 기대는지 보여 준다

live sidecar에서 이 값은 `confidence / source refs` 패널로 노출된다.

## 장면별 예시

### event 예시

기준 root:

- `artifacts/gui-smoke/endurance-longrun-20260404-live29/attempts/0001/steps/0027.request.json`

핵심 출력:

- `sceneType=event`
- `sceneStage=ancient-option`
- `canonicalOwner=event`
- `summaryText=이벤트 장면(ancient-option). 표시 옵션 3개, 식별된 이벤트 ancient-event.`

해석:

- 지금은 이벤트 화면이다
- ancient event의 option 단계다
- 사람이 실제로 누를 수 있는 옵션 3개가 보인다
- event identity도 이 경우는 잡혔다

### reward 예시

기준 root:

- `artifacts/gui-smoke/endurance-longrun-20260404-live28/attempts/0001/steps/0052.request.json`

핵심 출력:

- `sceneType=reward`
- `sceneStage=claim`
- `canonicalOwner=reward`
- reward entries 3개 + `넘기기`

해석:

- 지금은 보상 claim 단계다
- 사람이 볼 때 의미 있는 보상 항목 3개가 있다
- raw dump가 아니라 `gold / potion / card` 식의 정규화된 항목으로 정리된다

### rest-site 예시

기준 root:

- `artifacts/gui-smoke/endurance-longrun-20260404-live28/attempts/0001/steps/0101.request.json`

핵심 출력:

- `sceneType=rest-site`
- `sceneStage=release-pending`
- `canonicalOwner=rest-site`
- options: `휴식`, `재련`

해석:

- 지금은 단순 map이 아니라 rest-site owner가 아직 유지되는 장면이다
- canonical release stage는 `release-pending`이다
- 사람이 보는 선택지는 `휴식`, `재련` 두 개다

중요:

- 내부 canonical state와 눈에 보이는 선택지를 모두 보여 주기 때문에
- “지금 room owner가 남아 있는지”와 “사람이 뭘 보는지”를 함께 읽을 수 있다

### map 예시

기준 root:

- `artifacts/gui-smoke/endurance-longrun-20260404-live29/attempts/0001/steps/0033.request.json`

핵심 출력:

- `sceneType=map`
- `sceneStage=map-overlay`
- `canonicalOwner=map`
- reachable node 3개
- missing: `map-route-context-missing`, `map-current-node-identity-missing`

해석:

- 지금은 지도 overlay owner가 맞다
- 바로 갈 수 있는 노드 3개는 알 수 있다
- 하지만 더 먼 route context는 아직 모른다
- 즉 “지금 갈 수 있는 곳”은 말할 수 있지만, “장기 경로 조언”은 아직 제한적이다

## 이 문서를 읽을 때 주의할 점

1. summaryText만 보면 안 된다

- summary는 입구다
- 실제 판단은 `options`, `missingFacts`, `sceneStage`를 같이 봐야 한다

2. scene model은 recommendation이 아니다

- 이 문서는 “지금 무슨 상태인가”를 보여준다
- “무엇을 추천할까”는 다음 단계다

3. missingFacts는 실패가 아니라 정직성이다

- 없는 정보를 억지로 추정하지 않는 것이 현재 v1 원칙이다
- 따라서 missingFacts가 보인다고 해서 모델이 실패한 것은 아니다

4. request는 truth source가 아니다

- runId, attemptId, stepIndex 같은 envelope 정보는 request에서 읽지만
- scene truth는 `observer.state + canonical scene state`에서만 만든다

5. live sidecar도 recommendation panel이 아니다

- 이번 wave의 WPF 추가 패널은 read-only scene model viewer다
- AI recommendation과 fact model을 섞지 않는다

## 지금 당장 어디를 보면 되나

처음 읽는다면 아래 순서가 가장 좋다.

1. [ADVISOR_UI_COVERAGE_MATRIX_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/ADVISOR_UI_COVERAGE_MATRIX_KO.md)
2. [ADVISOR_SCENE_INFORMATION_MODEL.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/contracts/ADVISOR_SCENE_INFORMATION_MODEL.md)
3. 이 문서
4. 실제 `replay-advisor-scene` 출력 4개
   - event: `live29 step 0027`
   - reward: `live28 step 0052`
   - rest-site: `live28 step 0101`
   - map: `live29 step 0033`

## 한 줄 요약

```text
scene model은 “지금 장면이 무엇이고, 사람이 뭘 볼 수 있으며, 아직 무엇을 모르는가”를
AI 추천 이전 단계에서 안정적으로 설명하는 중간 표현이다.
```
